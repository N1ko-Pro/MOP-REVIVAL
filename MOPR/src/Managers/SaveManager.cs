// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Фасад системы сохранений со стабильным публичным API (его зовут Core и Items). Реальную работу
// делегирует классам MOPR.Saves: SaveAccess (теги), SaveProtection (read-only/бэкапы), SaveVerifier
// (целостность), BoltIntegrity (снимок болтов). Плюс родные MOP-проверки сборки Сатсумы (MSCEditor
// и полнота загрузки машины).

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MSCLoader;
using HutongGames.PlayMaker;
using MOPR.Common;
using MOPR.Saves;

namespace MOPR.Helpers
{
    internal static class SaveManager
    {
        // Доля затянутых болтов, ниже которой сборка считается подозрительной (MSCEditor).
        private const double MSCEditorLikelihoodRatio = 0.15;
        // Минимум установленных деталей, при котором проверка MSCEditor имеет смысл.
        private const int MinimumNumberOfPartsToCheckMSCEditorTampering = 40;

        private static readonly ES2Settings Setting = new ES2Settings();

        public static string SavePath => SaveAccess.SavePath;
        public static string ItemsPath => SaveAccess.ItemsPath;

        #region Защита файлов сохранения

        /// <summary>Снимает read-only и делает бэкап (в момент сохранения).</summary>
        public static void RemoveReadOnlyAttribute() => SaveProtection.OnGameSave();

        #endregion

        #region Теги сейва / предметов

        public static bool IsSaveTagPresent(string tag) => SaveAccess.Exists(tag);
        public static bool IsItemTagPresent(string tag) => SaveAccess.ItemExists(tag);

        public static void WriteSaveTag<T>(string tag, T value) => SaveAccess.WriteSave(tag, value);
        public static void WriteItemTag<T>(string tag, T value) => SaveAccess.WriteItem(tag, value);

        public static Transform ReadTransform(string tag)
        {
            SaveAccess.TryReadTransform(tag, out Transform value);
            return value;
        }

        public static Transform ReadItemTranform(string tag)
        {
            SaveAccess.TryReadItemTransform(tag, out Transform value);
            return value;
        }

        internal static Transform GetRadiatorHose3Transform() => ReadTransform("radiator hose3(xxxxx)");

        #endregion

        #region Верификация и жизненный цикл

        /// <summary>Проверка целостности сейва при загрузке.</summary>
        public static void VerifySave()
        {
            SaveProtection.OnGameLoad();
            SaveVerifier.Verify();
        }

        /// <summary>Фиксирует снимок затяжки болтов после загрузки.</summary>
        internal static void AddSaveFlag() => BoltIntegrity.Capture(false);

        /// <summary>Снимок болтов ведёт BoltIntegrity сам — освобождать нечего.</summary>
        internal static void ReleaseSave()
        {
        }

        #endregion

        #region Родные проверки сборки Сатсумы

        /// <summary>
        /// Не собрана ли машина сторонним софтом (MSCEditor): детали установлены и «затянуты», но
        /// фактические массивы болтов пусты.
        /// </summary>
        public static bool IsCarAssembledWithMSCEditor()
        {
            if (ModLoader.CurrentScene != CurrentScene.Game)
                throw new AccessViolationException("Can only be executed in-game.");

            int boltedCount = 0;
            int installedCount = 0;
            CountBoltedAndInstalled(GameObject.Find("Database/DatabaseBody"), ref boltedCount, ref installedCount);
            CountBoltedAndInstalled(GameObject.Find("Database/DatabaseMechanics"), ref boltedCount, ref installedCount);
            CountBoltedAndInstalled(GameObject.Find("Database/DatabaseMotor"), ref boltedCount, ref installedCount);

            if (installedCount < MinimumNumberOfPartsToCheckMSCEditorTampering)
            {
                ModConsole.Log("[MOPR] Only " + installedCount + " parts are installed.");
                return false;
            }

            double percentageOfPartsBolted = boltedCount / (double)installedCount;
            ModConsole.Log(string.Format("[MOPR] {0:0.00}% of Satsuma parts are installed, but not bolted.\n{1}/{2} parts are bolted and installed.",
                (1d - percentageOfPartsBolted) * 100, boltedCount, installedCount));

            return percentageOfPartsBolted <= MSCEditorLikelihoodRatio || IsEngineStupidlyAssembled();
        }

        private static void CountBoltedAndInstalled(GameObject databaseObject, ref int boltedCount, ref int installedCount)
        {
            if (databaseObject == null)
                return;

            foreach (PlayMakerFSM fsm in databaseObject.GetComponentsInChildren<PlayMakerFSM>())
            {
                FsmBool installed = fsm.FsmVariables.FindFsmBool("Installed");
                FsmBool bolted = fsm.FsmVariables.FindFsmBool("Bolted");
                if (bolted != null && installed != null && installed.Value)
                {
                    installedCount++;
                    if (bolted.Value)
                        boltedCount++;
                }
            }
        }

        /// <summary>Двигатель «собран по-глупому» (болты блока на нуле, но он установлен/затянут и не на подъёмнике).</summary>
        private static bool IsEngineStupidlyAssembled()
        {
            List<string> blockBolts = ReadStringList("BlockBolts");
            bool blockInstalled = ReadBoolean("block(Clone)Installed");
            bool blockBolted = ReadBoolean("block(Clone)Bolted");
            bool blockInHoist = ReadBoolean("block(Clone)InHoist");

            bool areAllBolted = true;
            foreach (string bolt in blockBolts)
                if (int.Parse(bolt.Replace("int(", "").Replace(")", "")) == 0)
                    areAllBolted = false;

            return blockInstalled && blockBolted && !areAllBolted && !blockInHoist;
        }

        /// <summary>Полностью ли загрузилась Сатсума (голова блока/блок/свечи консистентны). Бросает при несоответствии.</summary>
        public static bool IsSatsumaLoadedCompletely()
        {
            GameObject satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            if (satsuma == null)
                throw new NullReferenceException("Satsuma is missing");

            GameObject cylinderHead = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "cylinder head(Clone)");
            if (cylinderHead == null)
                throw new NullReferenceException("Cylinder head is missing");

            GameObject block = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "block(Clone)");
            if (block == null)
                throw new NullReferenceException("Block is missing");

            bool isCylinderHeadInstalled = ReadBoolean("cylinder head(Clone)Installed");
            bool isEngineBlockInstalled = ReadBoolean("block(Clone)Installed");

            if (!isEngineBlockInstalled)
            {
                if (isCylinderHeadInstalled && !cylinderHead.transform.Path().Contains("block(Clone)"))
                    throw new Exception("Cylinder head is not a part of the engine block.");
            }
            else
            {
                if ((!cylinderHead.gameObject.activeSelf || cylinderHead.transform.root != satsuma.transform) && isCylinderHeadInstalled)
                    throw new Exception("Cylinder head is not active, or is not a part of Satsuma.");
            }

            Transform sparkPlug1Pivot = cylinderHead.transform.Find("pivot_sparkplug1");
            if (sparkPlug1Pivot == null)
                throw new NullReferenceException("Unable to locate pivot_sparkplug1");

            for (int i = 1; i < 1000; i++)
            {
                if (ES2.Load<int?>(ItemsPath + "?tag=spark plug" + i + "TriggerID") == null)
                    break;

                if (ReadItemInt("spark plug" + i + "TriggerID") == 1
                    && ES2.Load<bool>(ItemsPath + "?tag=spark plug" + i + "Installed")
                    && sparkPlug1Pivot.childCount == 0)
                {
                    throw new Exception("Spark plug " + i + " is installed, but not a part of the engine.");
                }
            }

            return true;
        }

        #endregion

        #region ES2-хелперы для родных проверок

        private static bool ReadBoolean(string tag)
        {
            string path = SavePath + "?tag=" + tag;
            if (!ES2.Exists(path, Setting))
                throw new NullReferenceException("Boolean '" + tag + "' is not present in save file.");

            return ES2.Load<bool>(path, Setting);
        }

        private static List<string> ReadStringList(string tag)
        {
            string path = SavePath + "?tag=" + tag;
            if (!ES2.Exists(path, Setting))
                throw new NullReferenceException("List<String> '" + tag + "' is not present in the save file.");

            return ES2.LoadList<string>(path, Setting);
        }

        private static int ReadItemInt(string tag)
        {
            string path = ItemsPath + "?tag=" + tag;
            if (!ES2.Exists(path, Setting))
                throw new NullReferenceException("Int '" + tag + "' is not present in item save file.");

            return ES2.Load<int>(path, Setting);
        }

        #endregion
    }
}

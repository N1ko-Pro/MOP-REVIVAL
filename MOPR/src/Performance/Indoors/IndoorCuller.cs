// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Оптимизация в помещении: пока игрок внутри сектора (комната/магазин/гараж/камера — детекцию даёт
// SectorManager), скрываем тяжёлый наружный инертный декор, который стены и так загораживают
// (столбы, деревья, кусты, машинный зал, постройки, дальние пирсы/озеро). Каждый сектор оставляет в
// whitelist то, что видно из окна. Культим ТОЛЬКО инертные меш-группы — TRAFFIC/HUMANS/PERAJARVI и
// прочую логику не трогаем. Применение SetActive размазано по кадрам (бюджет групп за кадр), чтобы
// переход в помещение не давал микрофриз. Декор полностью возвращается при выходе, выключении
// настройки и остановке мода.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Managers;
using MOPR.Rules;
using MOPR.Rules.Types;

namespace MOPR.Indoors
{
    internal sealed class IndoorCuller : MonoBehaviour
    {
        // Групп, переключаемых за кадр. 1 — самое «мягкое»: тяжёлые SetActive больших групп декора
        // (деревья/кусты/столбы) не сходятся на один кадр, а расходятся по соседним, убирая микрофриз
        // при входе/выходе из помещения. ~24 группы → переход занимает доли секунды, визуально плавно.
        private const int PerFrameBudget = 1;

        // В профиле Quality эти группы не скрываем (сравнение по вхождению имени).
        private static readonly string[] QualityModeIgnore =
            { "RadioMast", "Tile", "LakeNice", "BUSHES", "PierHome", "TREES" };

        private readonly List<GameObject> objects = new List<GameObject>();
        private bool anyDisabled;

        private void Awake()
        {
            // Резолвим, пока группы ещё активны: GameObject.Find не находит выключенные объекты.
            Resolve();
        }

        private void Resolve()
        {
            objects.Clear();

            AddFind("ELEC_POLES");
            AddFind("ELEC_POLES_COLL");
            AddFind("TREES_MEDIUM3");
            AddFind("TREES_SMALL1");
            AddFind("TREES1_COLL");
            AddFind("TREES2_COLL");
            AddFind("TREES3_COLL");
            AddFind("BUSHES3");
            AddFind("BUSHES6");
            AddFind("BUSHES7");
            AddFind("BusStop");
            AddFind("BusStop 1");
            AddFind("MachineHall");
            AddFind("YARD/UNCLE/Shed");
            AddFind("YARD/UNCLE/Greenhouse");
            AddFind("YARD/UNCLE/LOD");
            AddFind("YARD/UNCLE/Home");
            AddFind("YARD/UNCLE/Building");
            AddFind("MAP/PierHome");
            AddFind("MAP/MESH/FOLIAGE/LAKE_VEGETATION");
            AddFind("MAP/RadioMast");
            AddFind("MAP/LakeSimple/Tile");
            // MAP/LakeNice несёт зеркальную поверхность/камеру отражений озера. Резолвим его всегда,
            // но под культинг он попадает ТОЛЬКО когда включена «Оптимизация отражений воды»
            // (см. DesiredActive). По умолчанию (оптимизация выключена) LakeNice остаётся активным,
            // иначе SetActive(false) погасил бы камеру отражений и вода стала бы плоской и статичной.
            // Плоская запасная вода LakeSimple/Tile отражений не даёт и культится безопасно.
            AddFind("MAP/LakeNice");

            // Правило ignore для группы выводит её из-под управления культингом.
            if (RulesManager.Instance != null && RulesManager.Instance.GetList<IgnoreRule>().Count > 0)
            {
                objects.RemoveAll(o => o != null &&
                    RulesManager.Instance.GetList<IgnoreRule>().Any(f => f.ObjectName == o.name));
            }

            if (MoprSettings.GenerateToggledItemsListDebug)
                ToggledItemsListGenerator.CreateSectorList(objects);
        }

        private void AddFind(string path)
        {
            GameObject go = GameObject.Find(path);
            if (go != null)
                objects.Add(go);
        }

        private void Update()
        {
            // Фича или мод выключены — весь декор должен быть виден.
            if (!MoprSettings.IsModActive || !MoprSettings.SectorCullingOn)
            {
                if (anyDisabled)
                    RestoreAll();

                return;
            }

            ApplyPaced();
        }

        private void ApplyPaced()
        {
            bool inSector = SectorManager.Instance != null && SectorManager.Instance.IsPlayerInSector();
            bool quality = MoprSettings.Mode == PerformanceMode.Quality;

            int budget = PerFrameBudget;
            anyDisabled = false;

            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj == null)
                    continue;

                bool desired = DesiredActive(obj, inSector, quality);

                if (obj.activeSelf != desired && budget > 0)
                {
                    obj.SetActive(desired);
                    budget--;
                }

                if (!obj.activeSelf)
                    anyDisabled = true;
            }
        }

        /// <summary>Должна ли группа быть активна: Quality-исключение и whitelist сектора → да, иначе «не в секторе».</summary>
        private static bool DesiredActive(GameObject obj, bool inSector, bool quality)
        {
            // Пока «Оптимизация отражений воды» выключена — LakeNice всегда активен (несёт камеру отражений озера).
            if (!MoprSettings.OptimizeWaterReflectionsOn && obj.name == "LakeNice")
                return true;

            if (quality && obj.name.ContainsAny(QualityModeIgnore))
                return true;

            if (SectorManager.Instance != null && SectorManager.Instance.SectorRulesContains(obj.name))
                return true;

            return !inSector;
        }

        private void RestoreAll()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];
                if (obj != null && !obj.activeSelf)
                    obj.SetActive(true);
            }

            anyDisabled = false;
        }

        private void OnDisable() => RestoreAll();
        private void OnDestroy() => RestoreAll();
    }
}

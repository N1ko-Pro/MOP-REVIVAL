// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// LOD-дубль: лёгкий визуальный клон-«подставка» целого объекта (транспорт, крупный предмет),
// показываемый вместо оригинала, пока тот выгружен вдалеке — чтобы на горизонте оставался силуэт.
// У MSC своего дистанционного LOD нет, поэтому подставку делаем сами: клонируем прототип, снимаем с
// клона всё «живое» (FSM, MonoBehaviour, коллайдеры, суставы, физика, звук, анимация), удаляем
// тяжёлые ветки и переименовываем детей в DUMMY_*, чтобы игра/другие моды не «утащили» части клона
// по имени. Клон делит меши/материалы с оригиналом.
//
// ВАЖНО (порядок загрузки): клон строится СИНХРОННО в конструкторе — на этапе регистрации сцены,
// пока прототип ещё активен и до общего ToggleAll(false). Позже клонировать нельзя: прототип уже
// выключен, и клон вышел бы пустым. Регистрация идёт под загрузочным экраном.

using System.Linq;
using UnityEngine;

using MOPR.Common;
using MOPR.Common.Enumerations;
using MOPR.Rules;
using MOPR.Rules.Types;

namespace MOPR.LOD
{
    internal class LodObject
    {
        public const string Prefix = "MOPR_Dumb-";
        public static readonly Vector3 LodStoragePosition = new Vector3(0, -100, 2137);
        public const int DefaultToggleDistance = 200;

        private readonly GameObject lodObject;

        /// <summary>Дистанция, после которой подставка переключается.</summary>
        public float ToggleDistance { get; set; } = DefaultToggleDistance;

        public LodObject(GameObject prototype)
        {
            if (prototype == null || IsLodDisabled(prototype))
                return;

            lodObject = (GameObject)Object.Instantiate(prototype, LodStoragePosition, Quaternion.Euler(0, 0, 0));
            lodObject.name = Prefix + prototype.name;

            if (Core.Instance != null && Core.Instance.DumbObjectParent != null)
                lodObject.transform.SetParent(Core.Instance.DumbObjectParent);

            StripLogic();
            lodObject.SetActive(false);
        }

        /// <summary>Правило no_lod для объекта или глобальный флаг NoLods отключают создание дубля.</summary>
        private static bool IsLodDisabled(GameObject prototype)
        {
            if (RulesManager.Instance == null)
                return false;

            if (RulesManager.Instance.SpecialRules.NoLods)
                return true;

            return RulesManager.Instance.GetList<NoLod>().FirstOrDefault(r => r.ObjectName == prototype.name) != null;
        }

        /// <summary>Снимает с клона всё «живое» и лишнее, оставляя дешёвый визуальный каркас.</summary>
        private void StripLogic()
        {
            DestroyAll<PlayMakerFSM>();
            DestroyAll<MonoBehaviour>();
            DestroyAll<Collider>();
            DestroyAll<Joint>();
            DestroyAll<Rigidbody>();
            DestroyAll<AudioSource>();
            DestroyAll<Animation>();

            // Оси и rigidbody на корне (клон транспорта).
            Axles axle = lodObject.GetComponent<Axles>();
            if (axle)
                Object.Destroy(axle);

            Rigidbody rootRigidbody = lodObject.GetComponent<Rigidbody>();
            if (rootRigidbody)
                Object.Destroy(rootRigidbody);

            DestroyMaskedElements();

            // Удаляем тяжёлые/логические ветки (набор выверен под Satsuma и прочий транспорт).
            DestroyGameObjects("LOD", "FuelTank", "DeadBody", "CoG", "Hose",
                "Dashboard", "Simulation", "ShitTank", "audio", "StagingWheel",
                "TrafficTrigger", "HookRear", "HookFront", "RadioPivot",
                "IKTarget", "PistonIK", "PlayerTrigger", "CarSimulation",
                "Interior", "Body/car body(xxxxx)/shadow_body", "MiscParts",
                "Electricity", "Wiring", "Wipers", "Colliders", "Chassis",
                "Sounds", "Bottle", "Body/Windshield", "Body/rear_windows",
                "Body/wiper_base", "Body/body masse(xxxxx)", "Body/cowl_parts",
                "Odometer", "HydraulicCylinder", "Automatic", "MESH/panel",
                "MESH/panel 1", "MESH/muscle_Scoop");

            // Переименовываем детей в DUMMY_*, чтобы имена не пересекались с живой сценой; заодно
            // удаляем выключенные и не несущие геометрии узлы.
            foreach (Transform t in lodObject.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject == lodObject)
                    continue;

                if (!t.gameObject.activeSelf || t.gameObject.GetComponentsInChildren<Renderer>(true).Length == 0)
                {
                    Object.Destroy(t.gameObject);
                    continue;
                }

                t.gameObject.name = "DUMMY_" + t.gameObject.name;
            }

            DisableShadowCasting();
        }

        /// <summary>Показывает/прячет подставку. При включении телепортирует в позу оригинала.</summary>
        public void ToggleActive(bool enabled, Transform transform)
        {
            if (lodObject == null || enabled == lodObject.activeSelf)
                return;

            lodObject.SetActive(enabled);
            if (!enabled)
                return;

            lodObject.transform.position = transform.position;
            lodObject.transform.eulerAngles = transform.eulerAngles;
        }

        /// <summary>Уничтожает клон-подставку.</summary>
        public void Destroy()
        {
            if (lodObject != null)
                Object.Destroy(lodObject);
        }

        private void DestroyAll<T>() where T : Component
        {
            foreach (T component in lodObject.GetComponentsInChildren<T>(true))
                Object.Destroy(component);
        }

        private void DestroyGameObjects(params string[] names)
        {
            foreach (string objectName in names)
            {
                Transform t = lodObject.transform.Find(objectName);
                if (t != null)
                    Object.Destroy(t.gameObject);
            }
        }

        /// <summary>Тени клона выключаем только в профиле Performance.</summary>
        private void DisableShadowCasting()
        {
            if (MoprSettings.Mode >= PerformanceMode.Balanced)
                return;

            foreach (MeshRenderer mesh in lodObject.GetComponentsInChildren<MeshRenderer>(true))
                mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            foreach (Projector projector in lodObject.GetComponentsInChildren<Projector>(true))
                Object.Destroy(projector);
        }

        /// <summary>Для Сатсумы: удаляем «Masked»-объекты клона.</summary>
        private void DestroyMaskedElements()
        {
            var masked = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(g => g.transform.root == lodObject.transform && g.name.StartsWith("Masked"));

            foreach (GameObject obj in masked)
                Object.Destroy(obj);
        }
    }
}

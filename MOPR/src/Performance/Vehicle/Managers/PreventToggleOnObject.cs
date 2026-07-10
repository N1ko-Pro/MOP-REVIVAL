// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Держатель узла, который нельзя выгружать вместе с машиной (аудио и т.п.): запоминает объект и его
// исходного родителя, чтобы на время выгрузки унести узел под временного родителя и вернуть обратно.

using UnityEngine;

namespace MOPR.Vehicles.Managers
{
    internal struct PreventToggleOnObject
    {
        public Transform ObjectTransform;
        public Transform OriginalParent;

        public PreventToggleOnObject(Transform objectTransform)
        {
            ObjectTransform = objectTransform;
            OriginalParent = objectTransform.parent;
        }
    }
}

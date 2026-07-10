// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Фикс массы сидений Сатсумы: пока сиденье не прикручено (затяжка ≤ 7), масса его rigidbody = 1,
// иначе — исходная. Иначе неприкрученное сиденье ведёт себя некорректно.

using UnityEngine;
using HutongGames.PlayMaker;
using MOPR.FSM;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaSeatsManager : MonoBehaviour
    {
        private Rigidbody rb;
        private FsmFloat tightness;
        private float defaultMass;
        private float lastTightnessValue;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            tightness = gameObject.GetPlayMaker("BoltCheck").FsmVariables.GetFsmFloat("Tightness");

            defaultMass = rb.mass;
            lastTightnessValue = tightness.Value;

            rb.mass = tightness.Value > 7 ? defaultMass : 1;
        }

        private void Update()
        {
            if (lastTightnessValue != tightness.Value)
            {
                rb.mass = tightness.Value > 7 ? defaultMass : 1;
                lastTightnessValue = tightness.Value;
            }
        }
    }
}

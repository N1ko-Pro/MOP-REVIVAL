// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Фикс сброса болтов Сатсумы: гасит рестарт FSM (BoltCheck/Use) и «приклеивает» сустав (бесконечная
// прочность на излом), пока деталь прикручена к машине. Unglue возвращает исходную прочность и
// самовыключается; Glue — жёстко фиксирует (при сохранении).

using UnityEngine;

using MOPR.FSM;
using MOPR.Common;
using MOPR.Vehicles.Cases;

namespace MOPR.Vehicles.Managers.SatsumaManagers
{
    internal class SatsumaBoltsAntiReload : MonoBehaviour
    {
        private readonly PlayMakerFSM fsm;

        private readonly float breakForce, breakTorque;
        private readonly FixedJoint fixedJoint;
        private readonly HingeJoint hingeJoint;

        private bool glued;

        public SatsumaBoltsAntiReload()
        {
            Satsuma.Instance.AddPart(this);

            try
            {
                string fsmName = gameObject.GetPlayMaker("BoltCheck") ? "BoltCheck" : "Use";
                fsm = gameObject.GetPlayMaker(fsmName);
                if (fsm == null)
                    return;

                fsm.Fsm.RestartOnEnable = false;

                fixedJoint = gameObject.GetComponent<FixedJoint>();
                hingeJoint = gameObject.GetComponent<HingeJoint>();

                if (fixedJoint)
                {
                    breakTorque = fixedJoint.breakTorque;
                    breakForce = fixedJoint.breakForce;
                }
                else if (hingeJoint)
                {
                    breakTorque = hingeJoint.breakTorque;
                    breakForce = hingeJoint.breakForce;
                }

                if (transform.root == Satsuma.Instance.transform)
                    glued = true;
            }
            catch (System.Exception ex)
            {
                ExceptionManager.New(ex, true, "BOLTS_ANTI_LOAD_SCRIPT_ERROR_" + gameObject.Path());
            }
        }

        private void Update()
        {
            if (!glued)
                return;

            if (fixedJoint)
                GlueFixedJoint();
            else if (hingeJoint)
                GlueHingeJoint();
        }

        private void GlueFixedJoint()
        {
            fixedJoint.breakTorque = Mathf.Infinity;
            fixedJoint.breakForce = Mathf.Infinity;
        }

        private void GlueHingeJoint()
        {
            hingeJoint.breakTorque = Mathf.Infinity;
            hingeJoint.breakForce = Mathf.Infinity;
        }

        public void Unglue()
        {
            glued = false;

            if (hingeJoint)
            {
                hingeJoint.breakForce = breakForce;
                hingeJoint.breakTorque = breakTorque;
            }
            else if (fixedJoint)
            {
                fixedJoint.breakForce = breakForce;
                fixedJoint.breakTorque = breakTorque;
            }

            // Скрипт больше не нужен (объект иногда может стать null).
            if (this != null)
                enabled = false;
        }

        public void Glue()
        {
            if (hingeJoint)
                GlueHingeJoint();

            if (fixedJoint)
                GlueFixedJoint();
        }
    }
}

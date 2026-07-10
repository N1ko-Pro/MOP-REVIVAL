// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен работы по ремонту лобового стекла: при входе в состояние просит SatsumaWindscreenFixer
// восстановить стекло Сатсумы.

using HutongGames.PlayMaker;
using MOPR.Vehicles.Managers.SatsumaManagers;

namespace MOPR.FSM.Actions
{
    internal class WindscreenRepairJob : FsmStateAction
    {
        private readonly SatsumaWindscreenFixer satsumaWindscreenFixer;

        public WindscreenRepairJob(SatsumaWindscreenFixer satsumaWindscreenFixer)
        {
            this.satsumaWindscreenFixer = satsumaWindscreenFixer;
        }

        public override void OnEnter()
        {
            satsumaWindscreenFixer.FixWindscreen();
        }
    }
}

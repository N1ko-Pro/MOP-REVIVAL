// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен: при поднятии трубки во время «большого» звонка Суски запускает отложенное пре-сохранение
// (игра в этот момент сохраняется — MOPR подготавливает сцену к записи).

using HutongGames.PlayMaker;

namespace MOPR.FSM.Actions
{
    public class CustomSuskiLargeFlip : FsmStateAction
    {
        public override void OnEnter()
        {
            Core.Instance.DelayedPreSave();
            Finish();
        }
    }
}

// MOPR — Modern Optimization Plugin (переработка мода MOP, Konrad Figura / Athlon). Revival by ANICKON. GPLv3.
//
// Экшен: воспроизводит звук сборки блока двигателя только если игрок рядом (≤5 м). Ссылки на
// объекты кэшируются при первом входе.

using HutongGames.PlayMaker;
using UnityEngine;

namespace MOPR.FSM.Actions
{
    public class MasterAudioAssembleCustom : FsmStateAction
    {
        private Transform player;
        private Transform thisTransform;
        private Transform masterAudioTransform;
        private AudioSource masterAudioSource;

        public override void OnEnter()
        {
            if (player == null)
            {
                player = GameObject.Find("PLAYER").transform;
                thisTransform = Fsm.GameObject.transform;
                masterAudioTransform = GameObject.Find("MasterAudio/CarBuilding/assemble").transform;
                masterAudioSource = masterAudioTransform.gameObject.GetComponent<AudioSource>();
            }

            if (Vector3.Distance(player.position, thisTransform.position) < 5)
            {
                masterAudioTransform.position = thisTransform.position;
                masterAudioSource.Play();
            }

            Finish();
        }
    }
}

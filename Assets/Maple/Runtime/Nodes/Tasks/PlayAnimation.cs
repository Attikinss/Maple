using UnityEngine;

namespace Maple.Nodes
{
    public sealed class PlayAnimation : Task
    {
        [Tooltip("The name of the animation to play."), SerializeField]
        private string m_AnimationName;

        [Tooltip("The layer of the animation begin played.\nSetting this to -1 will get the first layer with the animation within it."), Min(-1), SerializeField]
        private int m_AnimationLayer = 0;

        [Tooltip("If true the animation will crossfade from the currently played animation to the animation specified rather than \"Snapping\"."), SerializeField]
        private bool m_Crossfade = false;

        protected override void OnEnter() { }
        protected override void OnExit() { }

        protected override NodeResult OnTick()
        {
            Owner.Agent.PlayAnimation(m_AnimationName, m_AnimationLayer, m_Crossfade);

            return NodeResult.Success;
        }
    }
}
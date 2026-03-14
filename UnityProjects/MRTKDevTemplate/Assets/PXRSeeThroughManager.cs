using System.Threading.Tasks;
using Unity.XR.PXR;
using UnityEngine;

namespace PupilLabs.PICO
{
    public class PXRSeeThroughManager : MonoBehaviour
    {
        [SerializeField]
        private bool seeThroughEnabled = false;

        public bool SeeThroughEnabled { get { return seeThroughEnabled; } }

        private async void Start()
        {
            await Task.Delay(5000);
            PXR_Manager.EnableVideoSeeThrough = SeeThroughEnabled;
        }

        public void EnableSeeThrough(bool enabled)
        {
            PXR_Manager.EnableVideoSeeThrough = enabled;
            seeThroughEnabled = enabled;
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                PXR_Manager.EnableVideoSeeThrough = SeeThroughEnabled;
            }
        }
    }
}

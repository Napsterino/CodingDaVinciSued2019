using UnityEngine;
using UnityEngine.Networking;

namespace cdv
{
#pragma warning disable 618
    public sealed class InterpolatedTransform : NetworkBehaviour
#pragma warning restore 618
    {
        #region Shared Code
        void FixedUpdate()
        {
            if(isServer)
            {
                if((PreviousPosition - transform.position).magnitude > Threshold)
                {
                    MostRecentPosition = transform.position;
                    PreviousPosition = transform.position;
                }

                if(PreviousRotation != transform.rotation)
                {
                    MostRecentRotation = transform.rotation;
                    PreviousRotation = transform.rotation;
                }
            }
            else
            {
                if((MostRecentPosition - transform.position).magnitude > TeleportThreshold)
                {
                    transform.position = MostRecentPosition;
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, MostRecentPosition,
                    SmoothSpeed * Time.deltaTime);
                }

                transform.rotation = Quaternion.Slerp(transform.rotation, MostRecentRotation,
                    SmoothSpeed * Time.deltaTime);
            }
        }

#pragma warning disable 618
        [SyncVar] Vector3 MostRecentPosition;
        [SyncVar] Quaternion MostRecentRotation;
#pragma warning restore 618
        #endregion

        #region Client Code
        [SerializeField] float SmoothSpeed = 10;
        [SerializeField] float TeleportThreshold = 0.5f;
        #endregion

        #region Server Code
        Vector3 PreviousPosition;
        Quaternion PreviousRotation;
        [SerializeField] float Threshold = 0.001f;
        #endregion
    }
}


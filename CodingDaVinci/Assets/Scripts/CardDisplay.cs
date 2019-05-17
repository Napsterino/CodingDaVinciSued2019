using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace cdv
{
#pragma warning disable 618
    public class CardDisplay : NetworkBehaviour
#pragma warning restore 618
    {
        public void Hover(Vector3 targetPosition)
        {
            IsMoving = true;
            IsHoverd = true;
            TargetPosition = targetPosition;
        }

        public void UnHover()
        {
            IsMoving = true;
            IsHoverd = false;
            TargetPosition = OldPosition;
        }

        private void Update()
        {
            if(isServer)
            {
                if(IsMoving)
                {
                    transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime);
                    if(transform.position == TargetPosition)
                    {
                        IsMoving = false;
                    }
                }
            }
        }

        public Card Card;
        [SerializeField] private Text nameText;
        [SerializeField] private Text className;
        [SerializeField] private Text leaderAbility;
        [SerializeField] private Text playAbility;
        [SerializeField] private Image artwork;
#pragma warning disable 618
        [SyncVar] public bool IsHoverd;
        [SyncVar] public bool IsMoving;
        [SyncVar] Vector3 TargetPosition;
        [SyncVar] public Vector3 OldPosition;
#pragma warning restore 618

        void Start()
        {
            nameText.text = Card.name;
            className.text = Card.className;
            leaderAbility.text = Card.leaderAbility;
            playAbility.text = Card.playAbility;
            artwork.sprite = Card.artwork;
        }


    }

}

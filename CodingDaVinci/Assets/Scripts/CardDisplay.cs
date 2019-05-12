using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace cdv
{
#pragma warning disable 618
    public class CardDisplay : NetworkBehaviour
#pragma warning restore 618
    {

        [Command] private void CmdHover(Vector3 position)
        {
            IsMoving = true;
            m_IsHoverd = true;
            TargetPosition = position;
            OldPosition = transform.position;
        }

        [Command] private void CmdUnHover()
        {
            IsMoving = true;
            m_IsHoverd = false;
            TargetPosition = OldPosition;
        }

        public void Hover(Transform targetPostion)
        {
            CmdHover(targetPostion.position);
        }

        public void UnHover()
        {
            CmdUnHover();
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

        public bool IsHoverd => m_IsHoverd;

        public Card Card;
        [SerializeField] private Text nameText;
        [SerializeField] private Text className;
        [SerializeField] private Text leaderAbility;
        [SerializeField] private Text playAbility;
        [SerializeField] private Image artwork;
        [SyncVar] bool m_IsHoverd;
        [SyncVar] bool IsMoving;
        [SyncVar] Vector3 TargetPosition;
        [SyncVar] Vector3 OldPosition;

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

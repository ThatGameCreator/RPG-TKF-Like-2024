// Teleporter script for you gameobject - This is the controller script that will be included in your gameobject.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gyvr.Mythril2D
{

    public class TeleporterFlagBased : Entity, IObjectAction
    {
        [Header("General")]
        [SerializeField] GameObject m_teleporterGameObject = null;
        [SerializeField] bool m_enabled = false;

        [Header("Flag Activation")]
        [SerializeField] private string m_gameFlagID = string.Empty;


        [Header("Active Actions")]
        [SerializeField] private AudioClipResolver m_activationAudio;

        [Header("Animation")]
        [SerializeField] private string m_animationStateID = "state";


        protected Animator m_animator;
        protected BoxCollider2D m_boxCollider;

        protected void Awake()
        {
            Debug.Assert(m_teleporterGameObject);
            m_animator = this.GetComponent<Animator>();
            m_boxCollider = this.GetComponent<BoxCollider2D>();
            Debug.Assert(m_boxCollider);
        }


        protected override void Start()
        {
            base.Start();

            CheckFlagState();
        }

        // Return the state of the flag, plus it will 
        // switch the Teleporter object and collider of the interact accordingly
        protected virtual bool CheckFlagState()
        {
            m_enabled = GameManager.GameFlagSystem.Get(m_gameFlagID);
            SwitchInteractToTeleporter(m_enabled);
            return m_enabled;
        }


        // Switch between state of (is Teleporter) or (is interactable)
        // If teleporter: enable the teleporter gameobject (should be the child).
        // If interactable: enable the collider of the interactable.
        private void SwitchInteractToTeleporter(bool isTeleporter)
        {
            if (isTeleporter)
            {
                m_teleporterGameObject.SetActive(true);
                m_boxCollider.GetComponent<BoxCollider2D>().enabled = false;
            }
            else
            {
                // will set the teleport game object disappearance
                m_teleporterGameObject.SetActive(false);
                m_boxCollider.GetComponent<BoxCollider2D>().enabled = true;
            }
        }

        //Switches the state in the database (i.e. flag)
        //Plus, it interacts with graphic and sound 
        protected virtual bool SetFlagState(bool flagState, bool playAudio)
        {
            m_enabled = flagState;
            GameManager.GameFlagSystem.Set(m_gameFlagID, m_enabled);

            if (flagState && playAudio)
            {
                if (m_activationAudio != null)
                    GameManager.NotificationSystem.audioPlaybackRequested.Invoke(m_activationAudio);
            }

            if (m_animator != null)
            {
                m_animator.SetBool(m_animationStateID, flagState);
            }

            return CheckFlagState();
        }

        // Logic for when interacted
        // ObjectInteraction extension. You can perform operations depending on what 
        // state you receive
        public void SetState(EObjectAction state)
        {
            if (m_enabled) return;

            switch (state)
            {
                case EObjectAction.Activate:
                    SetFlagState(true, true);
                    break;
            }
        }
    }
}
/*


## All done, let's set it up our scene.
Create a empty game object and include has component the following:
*"TeleportFlagBase" script
* Animator
* Sprite Renderer
* Box Collider 2D

You should make an animation with the asset i'm including in this post. 
Copy one of the existent ones if it help you. 
Set a variable has bool and give it a name - i called it "state".

In your game object:
* create a teleporter object has child (just copy one of the existent ones in your scene). 
* In the your teleporterFlagBased script (in you main object) fill all the require fields. 
* Note that the "teleporter game object" is the child game object teleporter. 
* Also the "animation state id" is the id you game in your animation variable.
* setup the animator with your animation
* fill out the box collider2D. Note: 
* Your collider2D should be bigger than the collider of the child teleport, otherwise it will teleport before interact. 
* Set the layer to "Interaction" on the top right corner of your inspector.
* set the "interaction" to "command interaction", then in the command set to "Object Interaction"
* , and the object action it's the game object it self and the action it's "Activate".

THAT'S DONE!

Now if your really want to make it really fun, play with the interaction a little bit. I set dialogs and conditions for opening the teleport (like keys in the items, or an ability). Check the image.

Hope it helped.

*/
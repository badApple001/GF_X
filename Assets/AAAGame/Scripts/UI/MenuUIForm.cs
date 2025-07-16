
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;

namespace Game
{

    public partial class MenuUIForm : UIFormBase
    {
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

        }


        protected override void OnButtonClick(object sender, string btId)
        {
            base.OnButtonClick(sender, btId);
            if (btId == "Btn_Start")
            {
                Debug.Log("开始游戏");
                GFBuiltin.Event.Fire(this, EnterGameEventArgs.Create());
            }
        }
    }

}


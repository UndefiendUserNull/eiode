using EIODE.Components;
using EIODE.Resources;
using EIODE.Utils;
using Godot;

namespace EIODE.Scenes.Weapon
{
    public partial class MeleeWeaponBase : WeaponBase
    {
        [Export] public MeleeWeaponData Data { get; set; }
        public HitboxComponent Hitbox { get; set; }
        private Timer _hitboxTimer;
        private bool _canHit = false;

        public override void _Ready()
        {
            base._Ready();
            Hitbox = NodeUtils.GetChildWithNodeType<HitboxComponent>(this);
            Hitbox.Disable();
            _hitboxTimer = NodeUtils.GetChildWithNodeType<Timer>(this);
            _hitboxTimer.WaitTime = Data.HitRate;
            _hitboxTimer.Timeout += HitboxTimer_Timeout;
        }

        private void HitboxTimer_Timeout()
        {
            Hitbox.Disable();
            _canHit = true;
        }

        public override void Attack()
        {
            if (_canHit)
            {
                _canHit = false;
                Hitbox.Enable();
                _hitboxTimer.Start();
            }
        }

        public override WeaponAmmoData GetWeaponAmmoData()
        {
            return null;
        }

        public override WeaponData GetWeaponData()
        {
            return Data;
        }

        public override string GetWeaponName()
        {
            return Data.Name;
        }

        public override WeaponType GetWeaponType()
        {
            return Data.WeaponType;
        }
    }
}

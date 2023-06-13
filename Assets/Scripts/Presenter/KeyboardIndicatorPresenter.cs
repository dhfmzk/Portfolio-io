using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

using View;

namespace Presenter
{
    public class KeyboardIndicatorPresenter : MonoBehaviour
    {
        private PlayerControls _playerControls;

        public OnOffImageView onOffImageViewW;
        public OnOffImageView onOffImageViewA;
        public OnOffImageView onOffImageViewS;
        public OnOffImageView onOffImageViewD;
        
        public OnOffImageView onOffImageViewSpace;
        
        public OnOffImageView onOffImageViewE;
        
        private void Awake()
        {
            _playerControls = new PlayerControls();
            _playerControls.Player.Enable();
            _playerControls.Player.PlayerMovement.ReadValue<Vector2>();
            
            gameObject.UpdateAsObservable()
                .Select(_ => _playerControls.Player.PlayerMovement.ReadValue<Vector2>())
                .DistinctUntilChanged()
                .Subscribe(e =>
                {
                    onOffImageViewW.ToggleTo(e.y > 0);
                    onOffImageViewA.ToggleTo(e.x < 0);
                    onOffImageViewS.ToggleTo(e.y < 0);
                    onOffImageViewD.ToggleTo(e.x > 0);
                });
            
            gameObject.UpdateAsObservable()
                .Select(_ => _playerControls.Player.PlayerJump.ReadValue<float>())
                .Select(e => Math.Abs(e - 1) < 0.0000001f)
                .DistinctUntilChanged()
                .Subscribe(e =>
                {
                    onOffImageViewSpace.ToggleTo(e);
                });
            
            gameObject.UpdateAsObservable()
                .Select(_ => _playerControls.Player.PlayerInteraction.ReadValue<float>())
                .Select(e => Math.Abs(e - 1) < 0.0000001f)
                .DistinctUntilChanged()
                .Subscribe(e =>
                {
                    onOffImageViewE.ToggleTo(e);
                });
        }
    }
}

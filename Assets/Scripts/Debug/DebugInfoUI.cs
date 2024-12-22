using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Debug
{
    public class DebugInfoUI : MonoBehaviour
    {
        public TextMeshProUGUI screenSizeText;
        public TextMeshProUGUI canvasSizeText;
        public TextMeshProUGUI framePerSecondText;
        
        // Setting
        [SerializeField] private RectTransform mainCanvas;
        [SerializeField] private int samplingLength = 10;
        private readonly Queue<float> _timeQueue = new ();
        
        public void Update()
        {
            this.UpdateCanvasSize();
            this.UpdateResolution();
            this.UpdateFPS();
        }

        
        private void UpdateResolution()
        {
            this.screenSizeText.text = $"ScreenSize: {Screen.width} x {Screen.height}";
        }
        
        private void UpdateCanvasSize()
        {
            this.canvasSizeText.text = this.mainCanvas != null ? 
                $"CanvasSize: {this.mainCanvas.rect.width} x {this.mainCanvas.rect.height}" : 
                $"CanvasSize: U R FUCKED MAIN IS NULL";
        }
        
        private void UpdateFPS()
        {
            this._timeQueue.Enqueue(Time.deltaTime);
            if (this._timeQueue.Count > this.samplingLength)
            {
                this._timeQueue.Dequeue();
            }

            // 평균 시간으로 FPS 계산
            var totalTime = this._timeQueue.Sum();
            var averageTime = totalTime / this.samplingLength;
            var fps = 1.0f / averageTime;

            this.framePerSecondText.color = fps switch
            {
                < 28 => Color.red,
                < 58 => Color.yellow,
                _ => Color.green
            };

            this.framePerSecondText.text = $"FPS: {Mathf.Round(fps)}";
        }
    }
}

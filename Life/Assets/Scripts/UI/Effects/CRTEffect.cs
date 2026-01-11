#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// CRT/スキャンライン効果をUIに適用するコンポーネント
    /// Canvasの最前面に配置して使用
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class CRTEffect : MonoBehaviour
    {
        [Header("スキャンライン設定")]
        [SerializeField] private bool _enableScanlines = true;
        [SerializeField, Range(0f, 0.5f)] private float _scanlineIntensity = 0.1f;
        [SerializeField] private float _scanlineCount = 300f;
        [SerializeField] private float _scanlineSpeed = 0.5f;

        [Header("ビネット設定")]
        [SerializeField] private bool _enableVignette = true;
        [SerializeField, Range(0f, 1f)] private float _vignetteIntensity = 0.3f;
        [SerializeField, Range(0.01f, 1f)] private float _vignetteSmoothness = 0.5f;

        [Header("ノイズ/フリッカー設定")]
        [SerializeField] private bool _enableFlicker = true;
        [SerializeField, Range(0f, 0.1f)] private float _flickerIntensity = 0.02f;
        [SerializeField, Range(0f, 0.2f)] private float _noiseIntensity = 0.03f;

        [Header("全体設定")]
        [SerializeField, Range(0.5f, 1.5f)] private float _brightness = 1.0f;
        [SerializeField] private Color _tintColor = Color.white;

        private RawImage? _rawImage;
        private Material? _material;

        // シェーダープロパティID
        private static readonly int ScanlineIntensityId = Shader.PropertyToID("_ScanlineIntensity");
        private static readonly int ScanlineCountId = Shader.PropertyToID("_ScanlineCount");
        private static readonly int ScanlineSpeedId = Shader.PropertyToID("_ScanlineSpeed");
        private static readonly int VignetteIntensityId = Shader.PropertyToID("_VignetteIntensity");
        private static readonly int VignetteSmoothnessId = Shader.PropertyToID("_VignetteSmoothness");
        private static readonly int FlickerIntensityId = Shader.PropertyToID("_FlickerIntensity");
        private static readonly int NoiseIntensityId = Shader.PropertyToID("_NoiseIntensity");
        private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
            SetupMaterial();
        }

        private void Start()
        {
            UpdateMaterialProperties();
        }

        private void OnValidate()
        {
            if (_material != null)
            {
                UpdateMaterialProperties();
            }
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
            }
        }

        /// <summary>
        /// マテリアルをセットアップ
        /// </summary>
        private void SetupMaterial()
        {
            var shader = Shader.Find("LifeLike/UI/ScanlineOverlay");
            if (shader == null)
            {
                UnityEngine.Debug.LogError("[CRTEffect] シェーダーが見つかりません: LifeLike/UI/ScanlineOverlay");
                return;
            }

            _material = new Material(shader);
            if (_rawImage != null)
            {
                _rawImage.material = _material;
                _rawImage.color = Color.white;
            }
        }

        /// <summary>
        /// マテリアルプロパティを更新
        /// </summary>
        private void UpdateMaterialProperties()
        {
            if (_material == null) return;

            _material.SetFloat(ScanlineIntensityId, _enableScanlines ? _scanlineIntensity : 0f);
            _material.SetFloat(ScanlineCountId, _scanlineCount);
            _material.SetFloat(ScanlineSpeedId, _scanlineSpeed);
            _material.SetFloat(VignetteIntensityId, _enableVignette ? _vignetteIntensity : 0f);
            _material.SetFloat(VignetteSmoothnessId, _vignetteSmoothness);
            _material.SetFloat(FlickerIntensityId, _enableFlicker ? _flickerIntensity : 0f);
            _material.SetFloat(NoiseIntensityId, _enableFlicker ? _noiseIntensity : 0f);
            _material.SetFloat(BrightnessId, _brightness);
            _material.SetColor(TintColorId, _tintColor);
        }

        /// <summary>
        /// テーマから設定を適用
        /// </summary>
        public void ApplyTheme(UITheme theme)
        {
            _enableScanlines = theme.enableScanlines;
            _scanlineIntensity = theme.scanlineIntensity;
            _enableVignette = theme.enableVignette;
            _vignetteIntensity = theme.vignetteIntensity;
            UpdateMaterialProperties();
        }

        /// <summary>
        /// スキャンライン強度を設定
        /// </summary>
        public void SetScanlineIntensity(float intensity)
        {
            _scanlineIntensity = Mathf.Clamp01(intensity);
            if (_material != null)
            {
                _material.SetFloat(ScanlineIntensityId, _enableScanlines ? _scanlineIntensity : 0f);
            }
        }

        /// <summary>
        /// ビネット強度を設定
        /// </summary>
        public void SetVignetteIntensity(float intensity)
        {
            _vignetteIntensity = Mathf.Clamp01(intensity);
            if (_material != null)
            {
                _material.SetFloat(VignetteIntensityId, _enableVignette ? _vignetteIntensity : 0f);
            }
        }

        /// <summary>
        /// 効果の有効/無効を切り替え
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (_rawImage != null)
            {
                _rawImage.enabled = enabled;
            }
        }
    }
}

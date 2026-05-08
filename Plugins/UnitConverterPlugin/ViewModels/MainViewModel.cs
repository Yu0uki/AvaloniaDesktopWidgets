using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace UnitConverterPlugin.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] private int categoryIndex;
        [ObservableProperty] private int fromUnitIndex;
        [ObservableProperty] private int toUnitIndex;
        [ObservableProperty] private string inputValue = "1";
        [ObservableProperty] private string outputValue = "";

        public string[] Categories { get; } = { "长度", "重量", "温度" };

        public string[] LengthUnits { get; } = { "米 (m)", "千米 (km)", "厘米 (cm)", "毫米 (mm)", "英寸 (in)", "英尺 (ft)", "英里 (mi)" };
        public string[] WeightUnits { get; } = { "千克 (kg)", "克 (g)", "毫克 (mg)", "磅 (lb)", "盎司 (oz)" };
        public string[] TempUnits { get; } = { "摄氏度 (°C)", "华氏度 (°F)", "开尔文 (K)" };

        // Length: all to meters
        private static readonly double[] LengthToMeter = { 1.0, 1000.0, 0.01, 0.001, 0.0254, 0.3048, 1609.344 };
        // Weight: all to kilograms
        private static readonly double[] WeightToKg = { 1.0, 0.001, 0.000001, 0.453592, 0.0283495 };

        public string[] CurrentFromUnits => CategoryIndex switch
        {
            0 => LengthUnits, 1 => WeightUnits, 2 => TempUnits, _ => LengthUnits
        };

        public string[] CurrentToUnits => CurrentFromUnits;

        partial void OnCategoryIndexChanged(int value)
        {
            FromUnitIndex = 0; ToUnitIndex = 1;
            OnPropertyChanged(nameof(CurrentFromUnits));
            OnPropertyChanged(nameof(CurrentToUnits));
            Convert();
        }

        partial void OnFromUnitIndexChanged(int value) => Convert();
        partial void OnToUnitIndexChanged(int value) => Convert();
        partial void OnInputValueChanged(string value) => Convert();

        [RelayCommand]
        public void Convert()
        {
            if (!double.TryParse(InputValue, out var input))
            {
                OutputValue = "";
                return;
            }

            try
            {
                double result = CategoryIndex switch
                {
                    0 => ConvertLength(input),
                    1 => ConvertWeight(input),
                    2 => ConvertTemperature(input),
                    _ => 0
                };
                OutputValue = Math.Round(result, 6).ToString("0.######");
            }
            catch
            {
                OutputValue = "转换错误";
            }
        }

        private double ConvertLength(double val)
        {
            var meters = val * LengthToMeter[FromUnitIndex];
            return meters / LengthToMeter[ToUnitIndex];
        }

        private double ConvertWeight(double val)
        {
            var kg = val * WeightToKg[FromUnitIndex];
            return kg / WeightToKg[ToUnitIndex];
        }

        private double ConvertTemperature(double val)
        {
            // First to Celsius
            var celsius = FromUnitIndex switch
            {
                0 => val,
                1 => (val - 32) * 5.0 / 9.0,
                2 => val - 273.15,
                _ => val
            };
            // Then to target
            return ToUnitIndex switch
            {
                0 => celsius,
                1 => celsius * 9.0 / 5.0 + 32,
                2 => celsius + 273.15,
                _ => celsius
            };
        }

        [RelayCommand]
        public void SwapUnits()
        {
            (FromUnitIndex, ToUnitIndex) = (ToUnitIndex, FromUnitIndex);
        }
    }
}

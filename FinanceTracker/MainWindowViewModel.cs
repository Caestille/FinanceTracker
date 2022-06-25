using FinanceTracker.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FinanceTracker
{
	public class MainWindowViewModel
	{
		private const string lightModeKey = "Light";
		private const string darkModeKey = "Dark";

		private const string ColourModeSettingName = "ColourMode";
		private const string ThemeSettingName = "Theme";

		#region Colour stores
		// Background
		private static readonly Color MainBackgroundColourLight = Colors.White;
		private static readonly Color MainBackgroundColourDark = Colors.Black;

		// Datagrid header
		private static readonly Color DatagridHeaderColourLight = Color.FromArgb(255, 237, 237, 237);
		private static readonly Color DatagridHeaderColourDark = Color.FromArgb(255, 85, 85, 85);

		// Datagrid row
		private static readonly Color DatagridRowColourLight = Colors.White;
		private static readonly Color DatagridRowColourDark = Color.FromArgb(255, 30, 30, 30);

		// Menu bar
		private static readonly Color MenuBarColourLight = SystemColors.ControlLightColor;
		private static readonly Color MenuBarColourDark = Color.FromArgb(255, 51, 51, 51);

		// Menu popup
		private static readonly Color MenuPopupColourLight = Color.FromArgb(255, 241, 241, 241);
		private static readonly Color MenuPopupColourDark = Color.FromArgb(255, 73, 73, 73);

		// Slider thumb fill
		private static readonly Color SlideThumbFillColourLight = Color.FromArgb(255, 241, 241, 241);
		private static readonly Color SliderThumbFillColourDark = Color.FromArgb(255, 73, 73, 73);

		// Slider thumb outline
		private static readonly Color SliderThumbOutlineColourLight = Color.FromArgb(255, 150, 150, 150);
		private static readonly Color SliderThumbOutlineColourDark = Color.FromArgb(255, 150, 150, 150);

		// Slider in range fill
		private static readonly Color SliderInRangeFillColourLight = Color.FromArgb(255, 215, 215, 215);
		private static readonly Color SliderInRangeFillColourDark = Color.FromArgb(255, 51, 51, 51);

		// Slider in range outline
		private static readonly Color SliderInRangeOutlineColourLight = Color.FromArgb(255, 202, 202, 202);
		private static readonly Color SliderInRangeOutlineColourDark = Color.FromArgb(255, 60, 60, 60);

		// Combobox disabled background
		private static readonly Color ComboboxDisabledBackgroundColourLight = Color.FromArgb(255, 243, 243, 243);
		private static readonly Color ComboboxDisabledBackgroundColourDark = Color.FromArgb(255, 22, 22, 22);

		// Main text colour
		private static readonly Color TextColourLight = Colors.Black;
		private static readonly Color TextColourDark = Colors.White;

		// Status text colour
		private static readonly Color StatusTextColourLight = Colors.Gray;
		private static readonly Color StatusTextColourDark = Colors.DarkGray;

		// Status text light colour
		private static readonly Color StatusTextLightColourLight = Color.FromArgb(255, 225, 225, 225);
		private static readonly Color StatusTextLightColourDark = Color.FromArgb(255, 30, 30, 30);

		// Inverted text colour
		private static readonly Color InvertedTextColourLight = Colors.White;
		private static readonly Color InvertedTextColourDark = Colors.Black;

		// Tab selection button colour
		private static readonly Color TabButtonColourLight = Colors.Transparent;
		private static readonly Color TabButtonColourDark = Colors.Transparent;

		// Tab selection mouse over button colour
		private static readonly Color TabButtonMouseOverColourLight = Color.FromArgb(255, 245, 245, 245);
		private static readonly Color TabButtonMouseOverColourDark = Color.FromArgb(255, 14, 14, 14);

		// Tab selection click button colour
		private static readonly Color TabButtonClickColourLight = Color.FromArgb(255, 235, 235, 235);
		private static readonly Color TabButtonClickColourDark = Color.FromArgb(255, 24, 24, 24);

		// Theme colour
		private static Color ThemeColour = Color.FromArgb(255, 0, 122, 204);

		// Theme mouse over colour
		private static Color ThemeMouseOverColour = Color.FromArgb(255, 30, 134, 204);

		// Theme click colour
		private static Color ThemeClickColour = Color.FromArgb(255, 0, 103, 173);

		// Theme click colour
		private static Color ThemeBackgroundColour = Color.FromArgb(255, 129, 172, 202);

		// Theme text colour
		private static Color ThemeTextColour = Colors.White;

		// Theme disabled text colour
		private static readonly Color ThemeDisabledTextColourLight = Colors.White;
		private static readonly Color ThemeDisabledTextColourDark = Colors.Gray;

		#endregion

		private static bool isInDarkMode;

		private IRegistryService registryService;

		public MainWindowViewModel(
			IRegistryService registryService)
		{
			this.registryService = registryService;

			if (!registryService.TryGetSetting(ColourModeSettingName, out string? mode))
			{
				mode = lightModeKey;
				registryService.SetSetting(ColourModeSettingName, mode);
			}
			//if (mode == lightModeKey)
			//	LightModeMenuItem.IsChecked = true;
			//else if (mode == darkModeKey)
			//	DarkModeMenuItem.IsChecked = true;

			if (!registryService.TryGetSetting(ThemeSettingName, out string? theme))
			{
				theme = "255-0-122-204";
				registryService.SetSetting(ThemeSettingName, theme);
			}
			List<byte>? accent = theme?.Split('-').Select(byte.Parse).ToList();
			if (accent != null)
				SetAccentColour(accent[0], accent[1], accent[2], accent[3]);
		}

		private void lightModeMenuItem_Checked(/*object sender, RoutedEventArgs e*/)
		{
			registryService.SetSetting(ColourModeSettingName, lightModeKey);

			isInDarkMode = false;

			//if (DarkModeMenuItem != null)
			//	DarkModeMenuItem.IsChecked = false;

			SetThemeBackground();

			Application.Current.Resources["MainBackgroundBrush"] = new SolidColorBrush(MainBackgroundColourLight);
			Application.Current.Resources["DatagridHeaderBrush"] = new SolidColorBrush(DatagridHeaderColourLight);
			Application.Current.Resources["DatagridRowBrush"] = new SolidColorBrush(DatagridRowColourLight);
			Application.Current.Resources["MenuBarBrush"] = new SolidColorBrush(MenuBarColourLight);
			Application.Current.Resources["MenuPopupBrush"] = new SolidColorBrush(MenuPopupColourLight);
			Application.Current.Resources["SliderThumbFillBrush"] = new SolidColorBrush(SlideThumbFillColourLight);
			Application.Current.Resources["SliderThumbOutlineBrush"] = new SolidColorBrush(SliderThumbOutlineColourLight);
			Application.Current.Resources["SliderInRangeFillBrush"] = new SolidColorBrush(SliderInRangeFillColourLight);
			Application.Current.Resources["SliderInRangeOutlineBrush"] = new SolidColorBrush(SliderInRangeOutlineColourLight);
			Application.Current.Resources["TextBrush"] = new SolidColorBrush(TextColourLight);
			Application.Current.Resources["TextColour"] = Color.FromArgb(TextColourLight.A, TextColourLight.R, TextColourLight.G, TextColourLight.B);
			Application.Current.Resources["StatusTextBrush"] = new SolidColorBrush(StatusTextColourLight);
			Application.Current.Resources["StatusTextLightBrush"] = new SolidColorBrush(StatusTextLightColourLight);
			Application.Current.Resources["InvertedTextBrush"] = new SolidColorBrush(InvertedTextColourLight);
			Application.Current.Resources["TabButtonBrush"] = new SolidColorBrush(TabButtonColourLight);
			Application.Current.Resources["TabButtonMouseOverBrush"] = new SolidColorBrush(TabButtonMouseOverColourLight);
			Application.Current.Resources["TabButtonClickBrush"] = new SolidColorBrush(TabButtonClickColourLight);
			Application.Current.Resources["ComboboxDisabledBackgroundBrush"] = new SolidColorBrush(ComboboxDisabledBackgroundColourLight);
			Application.Current.Resources["ThemeDisabledTextBrush"] = new SolidColorBrush(ThemeDisabledTextColourLight);
		}

		private void darkModeMenuItem_Checked(/*object sender, RoutedEventArgs e*/)
		{
			registryService.SetSetting(ColourModeSettingName, darkModeKey);

			isInDarkMode = true;

			//if (LightModeMenuItem != null)
			//	LightModeMenuItem.IsChecked = false;

			SetThemeBackground();

			Application.Current.Resources["MainBackgroundBrush"] = new SolidColorBrush(MainBackgroundColourDark);
			Application.Current.Resources["DatagridHeaderBrush"] = new SolidColorBrush(DatagridHeaderColourDark);
			Application.Current.Resources["DatagridRowBrush"] = new SolidColorBrush(DatagridRowColourDark);
			Application.Current.Resources["MenuBarBrush"] = new SolidColorBrush(MenuBarColourDark);
			Application.Current.Resources["MenuPopupBrush"] = new SolidColorBrush(MenuPopupColourDark);
			Application.Current.Resources["SliderThumbFillBrush"] = new SolidColorBrush(SliderThumbFillColourDark);
			Application.Current.Resources["SliderThumbOutlineBrush"] = new SolidColorBrush(SliderThumbOutlineColourDark);
			Application.Current.Resources["SliderInRangeFillBrush"] = new SolidColorBrush(SliderInRangeFillColourDark);
			Application.Current.Resources["SliderInRangeOutlineBrush"] = new SolidColorBrush(SliderInRangeOutlineColourDark);
			Application.Current.Resources["TextBrush"] = new SolidColorBrush(TextColourDark);
			Application.Current.Resources["TextColour"] = Color.FromArgb(TextColourDark.A, TextColourDark.R, TextColourDark.G, TextColourDark.B);
			Application.Current.Resources["StatusTextBrush"] = new SolidColorBrush(StatusTextColourDark);
			Application.Current.Resources["StatusTextLightBrush"] = new SolidColorBrush(StatusTextLightColourDark);
			Application.Current.Resources["InvertedTextBrush"] = new SolidColorBrush(InvertedTextColourDark);
			Application.Current.Resources["TabButtonBrush"] = new SolidColorBrush(TabButtonColourDark);
			Application.Current.Resources["TabButtonMouseOverBrush"] = new SolidColorBrush(TabButtonMouseOverColourDark);
			Application.Current.Resources["TabButtonClickBrush"] = new SolidColorBrush(TabButtonClickColourDark);
			Application.Current.Resources["ComboboxDisabledBackgroundBrush"] = new SolidColorBrush(ComboboxDisabledBackgroundColourDark);
			Application.Current.Resources["ThemeDisabledTextBrush"] = new SolidColorBrush(ThemeDisabledTextColourDark);
		}

		private void accentSetterMenuItem_Click(/*object sender, RoutedEventArgs e*/)
		{
			//if (ColorPickerWPF.ColorPickerWindow.ShowDialog(out var color, ColorPickerWPF.Code.ColorPickerDialogOptions.SimpleView))
			//	SetAccentColour(color.A, color.R, color.G, color.B);
		}

		private void SetAccentColour(byte A, byte R, byte G, byte B)
		{
			registryService.SetSetting(ThemeSettingName, $"{A}-{R}-{G}-{B}");

			ThemeColour = Color.FromArgb(A, R, G, B);
			ThemeMouseOverColour = ChangeColorBrightness(ThemeColour, 0.1f);
			ThemeClickColour = ChangeColorBrightness(ThemeColour, -0.1f);
			SetThemeBackground();

			double perceivedBrightness = Math.Sqrt(0.299 * Math.Pow(R, 2) + 0.587 * Math.Pow(G, 2) + 0.114 * Math.Pow(B, 2));
			bool dark = perceivedBrightness < 255 * 0.5;

			ThemeTextColour = dark ? Colors.White : Colors.Black;

			Application.Current.Resources["ThemeBrush"] = new SolidColorBrush(ThemeColour);
			Application.Current.Resources["ThemeMouseOverBrush"] = new SolidColorBrush(ThemeMouseOverColour);
			Application.Current.Resources["ThemeClickBrush"] = new SolidColorBrush(ThemeClickColour);
			Application.Current.Resources["ThemeBackgroundBrush"] = new SolidColorBrush(ThemeBackgroundColour);
			Application.Current.Resources["ThemeTextBrush"] = new SolidColorBrush(ThemeTextColour);
		}

		private void SetThemeBackground()
		{
			float modifier = isInDarkMode ? -0.7f : 0.7f;
			ThemeBackgroundColour = ChangeColorBrightness(ThemeColour, modifier);
			Application.Current.Resources["ThemeBackgroundBrush"] = new SolidColorBrush(ThemeBackgroundColour);
		}

		public Color ChangeColorBrightness(Color color, float correctionFactor)
		{
			float red = (float)color.R;
			float green = (float)color.G;
			float blue = (float)color.B;

			if (correctionFactor < 0)
			{
				correctionFactor = 1 + correctionFactor;
				red *= correctionFactor;
				green *= correctionFactor;
				blue *= correctionFactor;
			}
			else
			{
				red = (255 - red) * correctionFactor + red;
				green = (255 - green) * correctionFactor + green;
				blue = (255 - blue) * correctionFactor + blue;
			}

			return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
		}
	}
}

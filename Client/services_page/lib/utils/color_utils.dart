import 'dart:ui';

class ColorUtils {
  
  /// convert string hex to color
  /// [code] => '#ffffff'
  static Color hexToColor(String code) {
    return Color(int.parse(code.substring(1, 7), radix: 16) + 0xFF000000);
  }
}
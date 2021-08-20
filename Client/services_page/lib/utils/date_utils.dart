import 'package:intl/intl.dart';

class DateUtils {
  /// format DateTime to String
  static String convertToString(DateTime time,
      {String format = 'HH:mma dd/MM/yyyy', bool convertUtc = true}) {
    DateFormat dateFormat = DateFormat(format);
    if (convertUtc) {
      return dateFormat.format(time.toUtc());
    } else {
      return dateFormat.format(time);
    }
  }

  /// format (DateTime) Issued date badge to String
  static String convertIssuedDateBadgeToString(DateTime time,
      {String format = 'dd/MM/yyyy', bool convertUtc = true}) {
    DateFormat dateFormat = DateFormat(format);
    if (convertUtc) {
      return dateFormat.format(time.toUtc());
    } else {
      return dateFormat.format(time);
    }
  }

  /// format DateTime to String
  static String convertStringUseForListEvent(DateTime time,
      {String format = 'HH:mma\ndd MMM yyyy', bool convertUtc = true}) {
    DateFormat dateFormat = DateFormat(format);
    if (convertUtc) {
      return dateFormat.format(time.toUtc());
    } else {
      return dateFormat.format(time);
    }
  }
}
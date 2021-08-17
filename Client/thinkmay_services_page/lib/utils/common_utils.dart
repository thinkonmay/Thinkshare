  
class CommonUtils {
  static String regexPhone() => r'(^(?:[+0]9)?[-0-9]{9,12}$)';

  static String regexEmail() =>
      r"^[a-zA-Z0-9.a-zA-Z0-9.!#$%&'*+=?^_`{|}~]+@[a-zA-Z0-9]+\.[a-zA-Z]+";

  static String regexPwd() => r"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,16}$";

  static String regexId() =>
      r"^(?=.{6,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$";

  static String regexAddress() => r"^[a-zA-Z0-9.a-zA-Z0-9.,]{4,100}";
}
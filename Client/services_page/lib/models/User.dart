class User{
  final int ID;
  final String UserName;
  final String DayOfBirth;
  final String Created;
  final String Token;

  User({
    this.ID,
    this.UserName,
    this.DayOfBirth,
    this.Created,
    this.Token,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      ID: json['ID'],
      UserName: json['UserName'],
      DayOfBirth: json['DayOfBirth'] as String,
      Created: json['Created'] as String,
      Token: json['Token'] as String,
    );
  }
}
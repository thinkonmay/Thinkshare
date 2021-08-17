class User {
  final int ID;
  final String FullName;
  final String DoB;
  final String Created;
  User({
    required this.ID,
    required this.FullName,
    required this.DoB,
    required this.Created,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
        ID: json['ID'],
        FullName: json['FullName'],
        DoB: json['DoB'],
        Created: json['Created']);
  }
}

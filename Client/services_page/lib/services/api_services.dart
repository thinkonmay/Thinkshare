import 'dart:convert';
import 'dart:io';
import 'package:admin/models/ServingSession.dart';
import 'package:admin/models/Slave.dart';
import 'package:admin/screens/login/login_screen.dart';
import 'package:admin/utils/server_config.dart';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

Future<List<Slave>> userFetchSlave() async {
   final response = await http.get(
      Uri.parse('$urlServer/User/FetchSlave'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
        'Authorization': 'Bearer $token'
      },
    );
  return compute(parseSlaves, response.body);
}

List<Slave> parseSlaves(String responseBody) {
  final parsed = jsonDecode(responseBody).cast<Map<String, dynamic>>();
  return parsed.map<Slave>((json) => Slave.fromJson(json)).toList();
}

Future<List<Slave>> userFetchSession() async {
   final response = await http.get(
      Uri.parse('$urlServer/User/FetchSession'),
      headers: <String, String>{
        'Content-Type': 'application/json; charset=UTF-8',
        'Authorization': 'Bearer $token'
      },
    );
  return compute(parseSession, response.body);
}

List<Slave> parseSession(String responseBody) {
  final parsed = jsonDecode(responseBody).cast<Map<String, dynamic>>();
  return parsed.map<Slave>((json) => Slave.fromJson(json)).toList();
}

class MyHttpOverrides extends HttpOverrides {
  @override
  HttpClient createHttpClient(SecurityContext context) {
    HttpClient client = super.createHttpClient(context);
    client.badCertificateCallback =
        (X509Certificate cert, String host, int port) => true;
    return client;
  }
}
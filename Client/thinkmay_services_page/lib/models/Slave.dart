import 'dart:convert';
import 'package:admin/services/api_services.dart';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

class Slave {
  final int ID;
  final String CPU;
  final String GPU;
  final int RAMcapacity;
  final String OS;

  Slave({
    required this.ID,
    required this.CPU,
    required this.GPU,
    required this.RAMcapacity,
    required this.OS,
  });

  factory Slave.fromJson(Map<String, dynamic> json) {
    return Slave(
      ID: json['ID'],
      CPU: json['CPU'],
      GPU: json['GPU'],
      RAMcapacity: json['RAMcapacity'],
      OS: json['OS'],
    );
  }
}
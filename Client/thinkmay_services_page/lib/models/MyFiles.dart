import 'package:admin/constants.dart';
import 'package:flutter/material.dart';

class CloudStorageInfo {
  final String? svgSrc, title, totalStorage, nameOfCPU, nameOfGPU;
  final int? numOfRam, percentage;
  final Color? color;

  CloudStorageInfo({
    this.svgSrc,
    this.title,
    this.totalStorage,
    this.nameOfCPU,
    this.nameOfGPU,
    this.numOfRam,
    this.percentage,
    this.color,
  });
}

List demoMyFiles = [
  CloudStorageInfo(
    title: "Device 1",
    numOfRam: 16,
    nameOfCPU: 'i5-6300U',
    nameOfGPU: 'NVIDIA GT 1030',
    svgSrc: "assets/icons/computer.svg",
    totalStorage: "256GB",
    color: primaryColor,
    percentage: 35,
  ),
  CloudStorageInfo(
    title: "Device 2",
    numOfRam: 8,
    nameOfCPU: 'i5-6300U',
    nameOfGPU: 'NVIDIA GT 1030',
    svgSrc: "assets/icons/computer.svg",
    totalStorage: "512GB",
    color: Color(0xFFFFA113),
    percentage: 35,
  ),
  CloudStorageInfo(
    title: "Device 3",
    numOfRam: 16,
    nameOfCPU: 'i5-6300U',
    nameOfGPU: 'NVIDIA GT 1030',
    svgSrc: "assets/icons/computer.svg",
    totalStorage: "256GB",
    color: Color(0xFF5f9de8),
    percentage: 10,
  ),
  CloudStorageInfo(
    title: "Device 4",
    numOfRam: 16,
      nameOfCPU: 'i5-6300U',
    nameOfGPU: 'NVIDIA GT 1030',
    svgSrc: "assets/icons/computer.svg",
    totalStorage: "1024GB",
    color: Color(0xFF007EE5),
    percentage: 78,
  ),
   CloudStorageInfo(
    title: "Device 5",
    numOfRam: 32,
        nameOfCPU: 'i5-6300U',
    nameOfGPU: 'NVIDIA GT 1030',
    svgSrc: "assets/icons/computer.svg",
    totalStorage: "512GB",
    color: primaryColor,
    percentage: 35,
  ),
  CloudStorageInfo(
    title: "Device 6",
    numOfRam: 1328,
        nameOfCPU: 'i5-6300U',
    nameOfGPU: 'NVIDIA GT 1030',
    svgSrc: "assets/icons/computer.svg",
    totalStorage: "512GB",
    color: Color(0xFFFFA113),
    percentage: 35,
  ),
  CloudStorageInfo(
    title: "Device 7",
    numOfRam: 1328,
        nameOfCPU: 'i5-6300U',
    nameOfGPU: 'NVIDIA GT 1030',
    svgSrc: "assets/icons/computer.svg",
    totalStorage: "256GB",
    color: Color(0xFF5f9de8),
    percentage: 10,
  ),
  CloudStorageInfo(
    title: "Device 8",
    numOfRam: 512,
        nameOfCPU: 'i5-6300U',
    nameOfGPU: 'NVIDIA GT 1030',
    svgSrc: "assets/icons/computer.svg",
    totalStorage: "256GB",
    color: Color(0xFF007EE5),
    percentage: 78,
  ),
];

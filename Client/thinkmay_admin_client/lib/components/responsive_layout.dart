import 'package:flutter/material.dart';

class ResponsiveLayout extends StatelessWidget {
  final Widget moble;
  final Widget tablelet;
  final Widget computer;
  const ResponsiveLayout({required Key key, required this.tablelet, required this.moble, required this.computer})
      : super(key: key);

  static int iphoneLimit = 600;
  static int ipadLimit = 1200;

  static bool isIphone(BuildContext context) =>
      MediaQuery.of(context).size.width < iphoneLimit;

  static bool isIpad(BuildContext context) =>
      MediaQuery.of(context).size.width < ipadLimit &&
      MediaQuery.of(context).size.width >= iphoneLimit;

  static bool isMacbook(BuildContext context) =>
      MediaQuery.of(context).size.width >= ipadLimit;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
        builder: (BuildContext context, BoxConstraints constraints) {
      if (constraints.maxWidth < iphoneLimit) {
        return moble;
      }
      if (constraints.maxWidth < ipadLimit) {
        return tablelet;
      } else {
        return computer;
      }
    });
  }
}
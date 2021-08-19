import 'package:flutter/material.dart';
import 'package:thinkmay_admin_client/screen/Drawer/src/TopContainer.dart';

class Drawder extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: Alignment.centerRight,
      child: Container(
        color: Color(0xfff7f7ff),
        height: MediaQuery.of(context).size.height,
        width: MediaQuery.of(context).size.width * 0.28,
        child: Column(children: [
          SizedBox(
            height: 30.0,
          ),
          TopContainer(),
        ]),
      ),
    );
  }
}
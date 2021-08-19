import 'package:flutter/material.dart';
import 'dart:html' as html;

class ProfileScreen extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: Center(
        child: Padding(
          padding: const EdgeInsets.only(bottom: 16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            mainAxisAlignment: MainAxisAlignment.center,
            children: <Widget>[
              SizedBox(
                height: 20,
              ),
              CircleAvatar(
                radius: 100,
                backgroundImage: AssetImage(
                  "assets/images/huyhoang.jpg",
                ),
              ),
              SizedBox(
                height: 10,
              ),
              Text(
                'Do Huy Hoang',
                style: Theme.of(context).textTheme.bodyText1!.copyWith(color: Colors.black),
                textScaleFactor: 2,
                textAlign: TextAlign.center,
              ),
              SizedBox(
                height: 20,
              ),
              Text(
                'Ông Vua Vật Lý. Chúa Tể Lập Trình\nRạp xiếc Trung Ương.',
                style: Theme.of(context).textTheme.caption,
                textScaleFactor: 2,
                textAlign: TextAlign.center,
              ),
              SizedBox(
                height: 40,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

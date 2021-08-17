import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:thinkmay_admin_client/components/responsive_layout.dart';

class TopContainer extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      // padding: EdgeInsets.symmetric(horizontal: 30.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: <Widget>[
          Row(
            children: [
              Container(
                margin: EdgeInsets.only(top: 8.0, left: 5.0),
                child: Text("Notification",
                    style: GoogleFonts.quicksand(
                        fontWeight: FontWeight.bold, fontSize: 20.0)),
              ),
            ],
          ),
          Container(
            margin: EdgeInsets.only(right: 18.0),
            padding: EdgeInsets.symmetric(
              horizontal: 16.0,
              vertical: 16.0 / 2,
            ),
            decoration: BoxDecoration(
              color: Color(0xFFb6d6f0),
              borderRadius: const BorderRadius.all(Radius.circular(10)),
              border: Border.all(color: Colors.white10),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                Image.asset(
                  "assets/images/kylan.png",
                  height: 38,
                ),
                SizedBox(height: 5.0),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0 / 2),
                  child: Text(
                    "Do Huy Hoang",
                    style: TextStyle(color: Colors.black),
                  ),
                ),
              ],
            ),
          )
        ],
      ),
    );
  }
}

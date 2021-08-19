import 'package:admin/components/AppTools.dart';
import 'package:admin/main.dart';
import 'package:admin/screens/main/main_screen.dart';
import 'package:flutter/material.dart';
import 'package:flutter_svg/flutter_svg.dart';

class SideMenu extends StatelessWidget {
  SideMenu({
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: ListView(
        children: [
          DrawerHeader(
            child: Image.asset("assets/images/logo_release.png"),
          ),
          DrawerListTile(
            title: "Dashboard",
            svgSrc: "assets/icons/menu_dashbord.svg",
            press: () {
              Navigator.push(context,
                  MaterialPageRoute(builder: (context) => MainScreen(1)));
            },
          ),
          DrawerListTile(
            title: "Documents",
            svgSrc: "assets/icons/menu_doc.svg",
            press: () {
              showMaterialDialog(
                  context,
                  "Trang này thêm vào để đỡ trống thôi, chả có gì đâu!",
                  "Du u wanna close me?",
                  'Close me!');
            },
          ),
          DrawerListTile(
            title: "Store",
            svgSrc: "assets/icons/menu_store.svg",
            press: () {
              showMaterialDialog(
                  context,
                  "Trang này thêm vào để đỡ trống thôi, chả có gì đâu!",
                  "Du u wanna close me?",
                  'Close me!');
            },
          ),
          DrawerListTile(
            title: "Notification",
            svgSrc: "assets/icons/menu_notification.svg",
            press: () {
              showMaterialDialog(
                  context,
                  "Trang này thêm vào để đỡ trống thôi, chả có gì đâu!",
                  "Du u wanna close me?",
                  'Close me!');
            },
          ),
          DrawerListTile(
            title: "Profile",
            svgSrc: "assets/icons/menu_profile.svg",
            press: () {
              Navigator.push(context,
                  MaterialPageRoute(builder: (context) => MainScreen(5)));
            },
          ),
          DrawerListTile(
            title: "Settings",
            svgSrc: "assets/icons/menu_setting.svg",
            press: () {
              showMaterialDialog(
                  context,
                  "Trang này thêm vào để đỡ trống thôi, chả có gì đâu!",
                  "Du u wanna close me?",
                  'Close me!');
            },
          ),
        ],
      ),
    );
  }
}

class DrawerListTile extends StatelessWidget {
  const DrawerListTile({
    Key? key,
    // For selecting those three line once press "Command+D"
    required this.title,
    required this.svgSrc,
    required this.press,
  }) : super(key: key);

  final String title, svgSrc;
  final VoidCallback press;
  @override
  Widget build(BuildContext context) {
    return ListTile(
      onTap: press,
      horizontalTitleGap: 0.0,
      leading: SvgPicture.asset(
        svgSrc,
        color: Colors.black,
        height: 16,
      ),
      title: Text(
        title,
        style: TextStyle(color: Colors.black),
      ),
    );
  }
}

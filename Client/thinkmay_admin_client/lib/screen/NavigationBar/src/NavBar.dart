import 'package:flutter/material.dart';
import 'package:flutter_vector_icons/flutter_vector_icons.dart';

class NavBar extends StatefulWidget {
  @override
  _NavBarState createState() => _NavBarState();
}

class _NavBarState extends State<NavBar> {
  List<bool> selected = [true, false, false, false, false];

  void select(int n) {
    for (int i = 0; i < 5; i++) {
      if (i != n) {
        selected[i] = false;
      } else {
        selected[i] = true;
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 350.0,
      child: Column(
        children: [
          NavBarItem(
            active: selected[0],
            icon: Feather.home,
            touched: () {
              setState(() {
                select(0);
              });
            },
          ),
          NavBarItem(
            active: selected[1],
            icon: Feather.list,
            touched: () {
              setState(() {
                select(1);
              });
            },
          ),
          NavBarItem(
            active: selected[2],
            icon: Feather.folder,
            touched: () {
              setState(() {
                select(2);
              });
            },
          ),
          NavBarItem(
            active: selected[3],
            icon: Feather.message_square,
            touched: () {
              setState(() {
                select(3);
              });
            },
          ),
          NavBarItem(
            active: selected[4],
            icon: Feather.settings,
            touched: () {
              setState(() {
                select(4);
              });
            },
          )
        ],
      ),
    );
  }
}

class NavBarItem extends StatefulWidget {
  final IconData icon;
  final Function touched;
  final bool active;

  NavBarItem({required this.active, required this.icon, required this.touched});
  @override
  _NavBarItemState createState() => _NavBarItemState();
}

class _NavBarItemState extends State<NavBarItem> {
  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: () {
          widget.touched();
        },
        splashColor: Colors.white,
        hoverColor: Colors.white12,
        child: Container(
          padding: EdgeInsets.symmetric(vertical: 3.0),
          child: Row(
            children: [
              Container(
                height: 60.0,
                width: 80.0,
                child: Row(
                  children: [
                    AnimatedContainer(
                      duration: Duration(milliseconds: 475),
                      height: 35.0,
                      width: 3.0,
                      decoration: BoxDecoration(
                        color:
                            widget.active ? Colors.white : Colors.transparent,
                        borderRadius: BorderRadius.only(
                            topRight: Radius.circular(10.0),
                            bottomRight: Radius.circular(10.0)),
                      ),
                    ),
                    Padding(
                        padding: EdgeInsets.only(left: 30.0),
                        child: Icon(
                          widget.icon,
                          color: widget.active ? Colors.white : Colors.white54,
                          size: 19.0,
                        ))
                  ],
                ),
              )
            ],
          ),
        ),
      ),
    );
  }
}
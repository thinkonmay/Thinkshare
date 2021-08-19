import 'package:admin/models/MyFiles.dart';
import 'package:admin/models/Slave.dart';
import 'package:admin/responsive.dart';
import 'package:admin/services/api_services.dart';
import 'package:flutter/material.dart';

import '../../../constants.dart';
import 'file_info_card.dart';

class MyFiles extends StatelessWidget {
  const MyFiles({
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final Size _size = MediaQuery.of(context).size;
    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              "My Devices",
              style: Theme.of(context)
                  .textTheme
                  .subtitle1!
                  .copyWith(color: Colors.black),
            ),
            ElevatedButton.icon(
              style: TextButton.styleFrom(
                padding: EdgeInsets.symmetric(
                  horizontal: defaultPadding * 1.5,
                  vertical:
                      defaultPadding / (Responsive.isMobile(context) ? 2 : 1),
                ),
              ),
              onPressed: () {},
              icon: Icon(Icons.add),
              label: Text("Add New",
                  style: TextStyle(
                    color: Colors.black,
                  )),
            ),
          ],
        ),
        SizedBox(height: defaultPadding),
        Responsive(
          mobile: FileInfoCardGridView(
            crossAxisCount: _size.width < 650 ? 2 : 4,
            childAspectRatio: _size.width < 650 ? 1.3 : 1,
          ),
          tablet: FileInfoCardGridView(),
          desktop: FileInfoCardGridView(
            childAspectRatio: _size.width < 1400 ? 1.1 : 1.4,
          ),
        ),
      ],
    );
  }
}

class FileInfoCardGridView extends StatelessWidget {
  const FileInfoCardGridView({
    Key? key,
    this.crossAxisCount = 3,
    this.childAspectRatio = 1,
  }) : super(key: key);

  final int crossAxisCount;
  final double childAspectRatio;

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<Slave>>(
        future: userFetchSlave(),
        builder: (context, snapshot) {
          print(snapshot.data);
          if (snapshot.hasData) {
            List<Slave>? slaves = snapshot.data;
            return GridView.count(
              physics: NeverScrollableScrollPhysics(),
              shrinkWrap: true,
              crossAxisCount: crossAxisCount,
              mainAxisSpacing: defaultPadding,
              crossAxisSpacing: defaultPadding,
              childAspectRatio: childAspectRatio,
              // // itemCount: slaves!.length,
              // gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
              //   crossAxisCount: crossAxisCount,
              //   crossAxisSpacing: defaultPadding,
              //   mainAxisSpacing: defaultPadding,
              //   childAspectRatio: childAspectRatio,
              // ),
              children: slaves!.map<Widget>((Slave slave) {
                return FileInfoCard(slave: slave);
              }).toList(),

              // itemBuilder: (context, index) => FileInfoCard(slave: slave),
            );
          } else {
            return Center(
              child: CircularProgressIndicator(),
            );
          }
        });
  }
}

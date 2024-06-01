DELETE FROM `weenie` WHERE `class_Id` = 607001;

INSERT INTO `weenie` (`class_Id`, `class_Name`, `type`, `last_Modified`)
VALUES (607001, 'ace607001-bindingrealm', 7, '2024-05-31 06:11:38') /* Portal */;

INSERT INTO `weenie_properties_int` (`object_Id`, `type`, `value`)
VALUES (607001,   1,      65536) /* ItemType - Portal */
     , (607001,  16,         32) /* ItemUseable - Remote */
     , (607001,  86,        100) /* MinLevel */
     , (607001,  93,       3084) /* PhysicsState - Ethereal, ReportCollisions, Gravity, LightingOn */
     , (607001, 111,         49) /* PortalBitmask - Unrestricted, NoSummon, NoRecall */
     , (607001, 133,          4) /* ShowableOnRadar - ShowAlways */;

INSERT INTO `weenie_properties_bool` (`object_Id`, `type`, `value`)
VALUES (607001,   1, True ) /* Stuck */
     , (607001,  12, True ) /* ReportCollisions */
     , (607001,  13, True ) /* Ethereal */
     , (607001,  14, True ) /* GravityStatus */
     , (607001,  15, True ) /* LightsStatus */
     , (607001,  19, True ) /* Attackable */
     , (607001,  88, True ) /* PortalShowDestination */;

INSERT INTO `weenie_properties_float` (`object_Id`, `type`, `value`)
VALUES (607001,  54, -0.10000000149011612) /* UseRadius */;

INSERT INTO `weenie_properties_string` (`object_Id`, `type`, `value`)
VALUES (607001,   1, 'Binding Realm') /* Name */
     , (607001,  38, 'Binding Realm') /* AppraisalPortalDestination */;

INSERT INTO `weenie_properties_d_i_d` (`object_Id`, `type`, `value`)
VALUES (607001,   1,   33555925) /* Setup */
     , (607001,   2,  150994947) /* MotionTable */
     , (607001,   8,  100667499) /* Icon */;

INSERT INTO `weenie_properties_position` (`object_Id`, `position_Type`, `obj_Cell_Id`, `origin_X`, `origin_Y`, `origin_Z`, `angles_W`, `angles_X`, `angles_Y`, `angles_Z`)
VALUES (607001, 2, 8323509, 250, -20, -22, 1, 0, 0, 0) /* Destination */
/* @teleloc 0x007F01B5 [250.000000 -20.000000 -22.000000] 1.000000 0.000000 0.000000 0.000000 */;

/* Lifestoned Changelog:
{
  "LastModified": "2024-05-30T23:10:51.6038939-07:00",
  "ModifiedBy": "pourman",
  "Changelog": [],
  "UserChangeSummary": "copied from fienos. Removed quest restriction",
  "IsDone": false
}
*/

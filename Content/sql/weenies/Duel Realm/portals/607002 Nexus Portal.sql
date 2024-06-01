DELETE FROM `weenie` WHERE `class_Id` = 607002;

INSERT INTO `weenie` (`class_Id`, `class_Name`, `type`, `last_Modified`)
VALUES (607002, 'ace607002-nexusportal', 7, '2024-06-01 05:59:16') /* Portal */;

INSERT INTO `weenie_properties_int` (`object_Id`, `type`, `value`)
VALUES (607002,   1,      65536) /* ItemType - Portal */
     , (607002,  16,         32) /* ItemUseable - Remote */
     , (607002,  93,       3084) /* PhysicsState - Ethereal, ReportCollisions, Gravity, LightingOn */
     , (607002, 111,          1) /* PortalBitmask - Unrestricted */
     , (607002, 133,          4) /* ShowableOnRadar - ShowAlways */;

INSERT INTO `weenie_properties_bool` (`object_Id`, `type`, `value`)
VALUES (607002,   1, True ) /* Stuck */
     , (607002,  11, False) /* IgnoreCollisions */
     , (607002,  12, True ) /* ReportCollisions */
     , (607002,  13, True ) /* Ethereal */
     , (607002,  14, True ) /* GravityStatus */
     , (607002,  15, True ) /* LightsStatus */
     , (607002,  19, True ) /* Attackable */
     , (607002,  88, True ) /* PortalShowDestination */;

INSERT INTO `weenie_properties_float` (`object_Id`, `type`, `value`)
VALUES (607002,  54, -0.10000000149011612) /* UseRadius */;

INSERT INTO `weenie_properties_string` (`object_Id`, `type`, `value`)
VALUES (607002,   1, 'Nexus Portal') /* Name */
     , (607002,  38, 'Nexus Portal') /* AppraisalPortalDestination */;

INSERT INTO `weenie_properties_d_i_d` (`object_Id`, `type`, `value`)
VALUES (607002,   1,   33555925) /* Setup */
     , (607002,   2,  150994947) /* MotionTable */
     , (607002,   8,  100667499) /* Icon */;

INSERT INTO `weenie_properties_position` (`object_Id`, `position_Type`, `obj_Cell_Id`, `origin_X`, `origin_Y`, `origin_Z`, `angles_W`, `angles_X`, `angles_Y`, `angles_Z`)
VALUES (607002, 2, 17826529, 40, -250, 24, 1, 0, 0, 0) /* Destination */
/* @teleloc 0x011002E1 [40.000000 -250.000000 24.000000] 1.000000 0.000000 0.000000 0.000000 */;

/* Lifestoned Changelog:
{
  "LastModified": "2024-05-31T22:59:02.9198198-07:00",
  "ModifiedBy": "pourman",
  "Changelog": [],
  "UserChangeSummary": "-Fixed Setup DID - portal was red not purple",
  "IsDone": true
}
*/

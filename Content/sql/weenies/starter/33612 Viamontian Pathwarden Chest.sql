DELETE FROM `weenie` WHERE `class_Id` = 33612;

INSERT INTO `weenie` (`class_Id`, `class_Name`, `type`, `last_Modified`)
VALUES (33612, 'ace33612-viamontianpathwardenchest', 20, '2024-03-30 06:31:13') /* Chest */;

INSERT INTO `weenie_properties_int` (`object_Id`, `type`, `value`)
VALUES (33612,   1,        512) /* ItemType - Container */
     , (33612,   5,      14750) /* EncumbranceVal */
     , (33612,   6,        120) /* ItemsCapacity */
     , (33612,   7,         10) /* ContainersCapacity */
     , (33612,  16,         48) /* ItemUseable - ViewedRemote */
     , (33612,  19,       2500) /* Value */
     , (33612,  38,       9999) /* ResistLockpick */
     , (33612,  82,          30) /* InitGeneratedObjects */
     , (33612,  93,       1048) /* PhysicsState - ReportCollisions, IgnoreCollisions, Gravity */
     , (33612, 100,          1) /* GeneratorType - Relative */
     , (33612, 173,          0) /* AppraisalLockpickSuccessPercent */;

INSERT INTO `weenie_properties_bool` (`object_Id`, `type`, `value`)
VALUES (33612,   1, True ) /* Stuck */
     , (33612,   2, False) /* Open */
     , (33612,   3, True ) /* Locked */
     , (33612,  11, True ) /* IgnoreCollisions */
     , (33612,  12, True ) /* ReportCollisions */
     , (33612,  14, True ) /* GravityStatus */
     , (33612,  19, True ) /* Attackable */;

INSERT INTO `weenie_properties_float` (`object_Id`, `type`, `value`)
VALUES (33612,  11,       1) /* ResetInterval */
     , (33612,  41,       1) /* RegenerationInterval */
     , (33612,  43,       1) /* GeneratorRadius */
     , (33612,  54,       1) /* UseRadius */;

INSERT INTO `weenie_properties_string` (`object_Id`, `type`, `value`)
VALUES (33612,   1, 'Viamontian Pathwarden Chest') /* Name */
     , (33612,  12, 'pathwardenchestkey') /* LockCode */
     , (33612,  14, 'Use this item to open it and see its contents.') /* Use */;

INSERT INTO `weenie_properties_d_i_d` (`object_Id`, `type`, `value`)
VALUES (33612,   1,   33554556) /* Setup */
     , (33612,   2,  150994948) /* MotionTable */
     , (33612,   3,  536870945) /* SoundTable */
     , (33612,   8,  100667424) /* Icon */
     , (33612,  22,  872415275) /* PhysicsEffectTable */;

INSERT INTO `weenie_properties_generator` (`object_Id`, `probability`, `weenie_Class_Id`, `delay`, `init_Create`, `max_Create`, `when_Create`, `where_Create`, `stack_Size`, `palette_Id`, `shade`, `obj_Cell_Id`, `origin_X`, `origin_Y`, `origin_Z`, `angles_W`, `angles_X`, `angles_Y`, `angles_Z`)
VALUES (33612, -1, 41513, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Pathwarden Trinket (41513) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 34257, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Asheron's Lesser Benediction (34257) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */;

/* Lifestoned Changelog:
{
  "LastModified": "2018-08-16T08:44:41.434142-07:00",
  "ModifiedBy": "fienos",
  "Changelog": [
    {
      "created": "2018-08-16T08:03:38.3092723-07:00",
      "author": "fienos",
      "comment": "Changed Name of Chest to Viamontian Pathwarden Chest"
    }
  ],
  "UserChangeSummary": null,
  "IsDone": false
}
*/

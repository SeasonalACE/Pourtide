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
     , (33612, -1, 70223, 0, 2, 2, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Claw (70223) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45958, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Baton (45958) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45954, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate  (45954) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45932, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate  (45932) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45906, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Compound Bow (45906) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45936, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate  (45936) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45908, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate  (45908) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45912, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate  (45912) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45930, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate  (45930) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45934, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate  (45934) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45944, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate  (45944) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45956, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Nether Staff (45956) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45961, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Breastplate (45961) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45963, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Gauntlets (45963) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45965, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Girth (45965) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45967, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Greaves (45967) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45969, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Helm (45969) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45971, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Pauldrons (45971) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45973, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate  (45973) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45977, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Tassets (45977) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45979, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Vambraces (45979) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 45975, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Vambraces (45979) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 23356, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Vambraces (45979) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 7595, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Vambraces (45979) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 15408, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Vambraces (45979) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */
     , (33612, -1, 34257, 0, 1, 1, 2, 8, -1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0) /* Generate Seasoned Explorer Vambraces (45979) (x1 up to max of 1) - Regenerate upon PickUp - Location to (re)Generate: Contain */


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
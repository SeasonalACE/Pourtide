use ace_shard;

ALTER TABLE biota_properties_allegiance DROP FOREIGN KEY FK_allegiance_biota_Id;
ALTER TABLE biota_properties_anim_part DROP FOREIGN KEY wcid_animpart;
ALTER TABLE biota_properties_attribute DROP FOREIGN KEY wcid_attribute;
ALTER TABLE biota_properties_attribute_2nd DROP FOREIGN KEY wcid_attribute2nd;
ALTER TABLE biota_properties_body_part DROP FOREIGN KEY wcid_bodypart;
ALTER TABLE biota_properties_book DROP FOREIGN KEY wcid_bookdata;
ALTER TABLE biota_properties_book_page_data DROP FOREIGN KEY wcid_pagedata;
ALTER TABLE biota_properties_bool DROP FOREIGN KEY wcid_bool;
ALTER TABLE biota_properties_create_list DROP FOREIGN KEY wcid_createlist;
ALTER TABLE biota_properties_d_i_d DROP FOREIGN KEY wcid_did;
ALTER TABLE biota_properties_emote DROP FOREIGN KEY wcid_emote;
ALTER TABLE biota_properties_enchantment_registry DROP FOREIGN KEY wcid_enchantmentregistry;
ALTER TABLE biota_properties_event_filter DROP FOREIGN KEY wcid_eventfilter;
ALTER TABLE biota_properties_float DROP FOREIGN KEY wcid_float;
ALTER TABLE biota_properties_generator DROP FOREIGN KEY wcid_generator;
ALTER TABLE biota_properties_i_i_d DROP FOREIGN KEY wcid_iid;
ALTER TABLE biota_properties_int DROP FOREIGN KEY wcid_int;
ALTER TABLE biota_properties_int64 DROP FOREIGN KEY wcid_int64;
ALTER TABLE biota_properties_palette DROP FOREIGN KEY wcid_palette;
ALTER TABLE biota_properties_position DROP FOREIGN KEY wcid_position;
ALTER TABLE biota_properties_skill DROP FOREIGN KEY wcid_skill;
ALTER TABLE biota_properties_spell_book DROP FOREIGN KEY wcid_spellbook;
ALTER TABLE biota_properties_string DROP FOREIGN KEY wcid_string;
ALTER TABLE biota_properties_texture_map DROP FOREIGN KEY wcid_texturemap;
ALTER TABLE house_permission DROP FOREIGN KEY biota_Id_house_Id;

UPDATE ace_shard.biota b
SET b.id = (393216 << 32) | b.id
WHERE b.id >= 0x70000000 AND b.id < 0x80000000;

UPDATE ace_shard.biota_properties_bool b
SET b.object_Id = (393216 << 32) | b.object_Id 
WHERE b.object_Id >= 0x70000000 AND b.object_Id < 0x80000000;

UPDATE ace_shard.biota_properties_create_list b
SET b.object_Id = (393216 << 32) | b.object_Id 
WHERE b.object_Id >= 0x70000000 AND b.object_Id < 0x80000000;

UPDATE ace_shard.biota_properties_d_i_d b
SET b.object_Id = (393216 << 32) | b.object_Id 
WHERE b.object_Id >= 0x70000000 AND b.object_Id < 0x80000000;

UPDATE ace_shard.biota_properties_i_i_d b
SET b.object_Id = (393216 << 32) | b.object_Id 
WHERE b.object_Id >= 0x70000000 AND b.object_Id < 0x80000000;

UPDATE ace_shard.biota_properties_i_i_d b
SET b.value = (393216 << 32) | b.value 
WHERE b.value >= 0x70000000 AND b.value < 0x80000000;

UPDATE ace_shard.biota_properties_float b
SET b.object_Id = (393216 << 32) | b.object_Id 
WHERE b.object_Id >= 0x70000000 AND b.object_Id < 0x80000000;

UPDATE ace_shard.biota_properties_int b
SET b.object_Id = (393216 << 32) | b.object_Id 
WHERE b.object_Id >= 0x70000000 AND b.object_Id < 0x80000000;

UPDATE ace_shard.biota_properties_int64 b
SET b.object_Id = (393216 << 32) | b.object_Id 
WHERE b.object_Id >= 0x70000000 AND b.object_Id < 0x80000000;

UPDATE ace_shard.biota_properties_position b
SET b.object_Id = (393216 << 32) | b.object_Id 
WHERE b.object_Id >= 0x70000000 AND b.object_Id < 0x80000000;

UPDATE ace_shard.biota_properties_string b
SET b.object_Id = (393216 << 32) | b.object_Id 
WHERE b.object_Id >= 0x70000000 AND b.object_Id < 0x80000000;

UPDATE ace_shard.house_permission h
SET h.house_Id = (393216 << 32) | h.house_Id 
WHERE h.house_Id >= 0x70000000 AND h.house_Id < 0x80000000;

ALTER TABLE `biota_properties_allegiance` ADD CONSTRAINT `FK_allegiance_biota_Id` FOREIGN KEY(allegiance_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_anim_part` ADD CONSTRAINT `wcid_animpart` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_attribute` ADD CONSTRAINT `wcid_attribute` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_attribute_2nd` ADD CONSTRAINT `wcid_attribute2nd` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_body_part` ADD CONSTRAINT `wcid_bodypart` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_book` ADD CONSTRAINT `wcid_bookdata` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_book_page_data` ADD CONSTRAINT `wcid_pagedata` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_bool` ADD CONSTRAINT `wcid_bool` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_create_list` ADD CONSTRAINT `wcid_createlist` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_d_i_d` ADD CONSTRAINT `wcid_did` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_emote` ADD CONSTRAINT `wcid_emote` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_enchantment_registry` ADD CONSTRAINT `wcid_enchantmentregistry` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_event_filter` ADD CONSTRAINT `wcid_eventfilter` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_float` ADD CONSTRAINT `wcid_float` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_generator` ADD CONSTRAINT `wcid_generator` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_i_i_d` ADD CONSTRAINT `wcid_iid` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_int` ADD CONSTRAINT `wcid_int` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_int64` ADD CONSTRAINT `wcid_int64` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_palette` ADD CONSTRAINT `wcid_palette` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_position` ADD CONSTRAINT `wcid_position` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_skill` ADD CONSTRAINT `wcid_skill` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_spell_book` ADD CONSTRAINT `wcid_spellbook` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_string` ADD CONSTRAINT `wcid_string` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `biota_properties_texture_map` ADD CONSTRAINT `wcid_texturemap` FOREIGN KEY(object_Id) REFERENCES biota(id) ON DELETE CASCADE;
ALTER TABLE `house_permission` ADD CONSTRAINT `biota_Id_house_Id` FOREIGN KEY(house_Id) REFERENCES biota(id) ON DELETE CASCADE;
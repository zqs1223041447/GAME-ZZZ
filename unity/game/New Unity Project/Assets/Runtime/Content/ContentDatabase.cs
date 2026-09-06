using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Runtime.Content
{
    public sealed class ContentDatabase
    {
        public static ContentDatabase Empty { get; } = new ContentDatabase(Array.Empty<ContentRecord>());

        readonly Dictionary<string, ContentRecord> _byId;

        ContentDatabase(IReadOnlyList<ContentRecord> records)
        {
            _byId = new Dictionary<string, ContentRecord>(records.Count, StringComparer.Ordinal);
            for (int i = 0; i < records.Count; i++)
                _byId.Add(records[i].Id.Value, records[i]);
        }

        public int Count
        {
            get { return _byId.Count; }
        }

        public bool TryGet(ContentId id, out ContentRecord record)
        {
            if (id.Value == null)
            {
                record = null;
                return false;
            }

            return _byId.TryGetValue(id.Value, out record);
        }

        /// <summary>
        /// Load top-level *.json in <paramref name="directory"/>. Any error fails the whole load.
        /// </summary>
        public static bool TryLoadAll(string directory, out ContentDatabase db, out List<string> errors)
        {
            errors = new List<string>();
            db = Empty;

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                errors.Add((directory ?? string.Empty) + ": 目录不存在");
                return false;
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                errors.Add(directory + ": 无法读取目录 (" + ex.Message + ")");
                return false;
            }

            Array.Sort(files, StringComparer.Ordinal);
            var parsed = new List<ContentRecord>(files.Length);
            var seen = new Dictionary<string, string>(StringComparer.Ordinal);

            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i];
                string fileName = Path.GetFileName(path);
                string json;
                try
                {
                    json = File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    errors.Add(fileName + ": JSON 损坏 (" + ex.Message + ")");
                    continue;
                }

                ContentJson dto;
                try
                {
                    dto = JsonUtility.FromJson<ContentJson>(json);
                }
                catch (Exception ex)
                {
                    errors.Add(fileName + ": JSON 损坏 (" + ex.Message + ")");
                    continue;
                }

                if (dto == null)
                {
                    errors.Add(fileName + ": JSON 损坏");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(dto.id))
                {
                    errors.Add(fileName + ": 缺 id");
                    continue;
                }

                if (!ContentId.TryParse(dto.id, out ContentId id))
                {
                    errors.Add(fileName + ": id 不含点");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(dto.displayName))
                {
                    errors.Add(fileName + ": 缺 displayName");
                    continue;
                }

                if (seen.ContainsKey(id.Value))
                {
                    errors.Add(fileName + ": 重复 id '" + id.Value + "'");
                    continue;
                }

                seen.Add(id.Value, fileName);
                parsed.Add(new ContentRecord(id, dto.displayName.Trim()));
            }

            if (errors.Count > 0)
            {
                db = Empty;
                return false;
            }

            db = new ContentDatabase(parsed);
            return true;
        }

        [Serializable]
        class ContentJson
        {
            public string id;
            public string displayName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VoxelDestructionPro.Tools
{
    public class RagdollMotionRecorder : MonoBehaviour
    {
        [Serializable]
        private class BonePose
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
        }

        [Serializable]
        private class Frame
        {
            public float time;
            public Vector3 rootPositionDelta;
            public Quaternion rootRotationDelta;
            public BonePose[] bonePoses;
        }

        [Serializable]
        private class RecordingData
        {
            public float sampleRate;
            public string recordedAtUtc;
            public string[] bonePaths;
            public Frame[] frames;
        }

        [Header("Rig")]
        [SerializeField] private Transform root;
        [SerializeField] private Transform[] bones;

        [Header("Recording")]
        [SerializeField] private float sampleRate = 60f;
        [SerializeField] private bool recordOnStart;
        [SerializeField] private string recordingFileName = "ragdoll_recording.json";
        [SerializeField] private bool saveToDownloadsOnStop = true;

        [Header("Playback")]
        [SerializeField] private TextAsset playbackJson;
        [SerializeField] private bool playOnStart;

        private readonly List<Frame> recordedFrames = new List<Frame>();
        private string[] recordedBonePaths;
        private float nextSampleTime;
        private bool isRecording;
        private bool isPlaying;
        private float recordingStartTime;
        private Vector3 recordingStartPosition;
        private Quaternion recordingStartRotation;
        private Vector3 playbackStartPosition;
        private Quaternion playbackStartRotation;
        private Frame[] playbackFrames;
        private int playbackIndex;
        private float playbackTime;
        private Transform[] playbackBones;

        private void Awake()
        {
            if (root == null)
            {
                root = transform;
            }
        }

        private void Start()
        {
            if (recordOnStart)
            {
                StartRecording();
            }

            if (playOnStart)
            {
                StartPlayback();
            }
        }

        private void Update()
        {
            if (isRecording)
            {
                TrySampleRecording();
            }

            if (isPlaying)
            {
                TickPlayback();
            }
        }

        public void StartRecording()
        {
            if (root == null)
            {
                Debug.LogWarning("RagdollMotionRecorder: Root transform is not assigned.");
                return;
            }

            EnsureBones();
            recordedFrames.Clear();
            recordedBonePaths = BuildBonePaths();
            recordingStartTime = Time.time;
            nextSampleTime = recordingStartTime;
            recordingStartPosition = root.position;
            recordingStartRotation = root.rotation;
            isRecording = true;
            isPlaying = false;
        }

        public void StopRecording()
        {
            isRecording = false;

            if (saveToDownloadsOnStop)
            {
                SaveRecordingToDownloads();
            }
        }

        public void StartPlayback()
        {
            if (!LoadPlaybackData())
            {
                Debug.LogWarning("RagdollMotionRecorder: No playback data loaded.");
                return;
            }

            EnsurePlaybackBones();

            if (playbackBones == null || playbackBones.Length == 0)
            {
                Debug.LogWarning("RagdollMotionRecorder: No playback bones found.");
                return;
            }

            playbackStartPosition = root != null ? root.position : transform.position;
            playbackStartRotation = root != null ? root.rotation : transform.rotation;
            playbackIndex = 0;
            playbackTime = 0f;
            isPlaying = true;
            isRecording = false;
        }

        public void StopPlayback()
        {
            isPlaying = false;
        }

        public void SaveRecordingToDownloads()
        {
            if (recordedFrames.Count == 0)
            {
                Debug.LogWarning("RagdollMotionRecorder: No frames recorded.");
                return;
            }

            RecordingData data = new RecordingData
            {
                sampleRate = sampleRate,
                recordedAtUtc = DateTime.UtcNow.ToString("O"),
                bonePaths = recordedBonePaths,
                frames = recordedFrames.ToArray()
            };

            string json = JsonUtility.ToJson(data, true);
            string targetPath = ResolveDownloadsPath();
            string filePath = Path.Combine(targetPath, recordingFileName);
            Directory.CreateDirectory(targetPath);
            File.WriteAllText(filePath, json);
            Debug.Log($"RagdollMotionRecorder: Saved recording to {filePath}");
        }

        private void TrySampleRecording()
        {
            if (Time.time < nextSampleTime)
            {
                return;
            }

            float elapsed = Time.time - recordingStartTime;
            Vector3 rootDelta = root.position - recordingStartPosition;
            Quaternion rootDeltaRot = Quaternion.Inverse(recordingStartRotation) * root.rotation;

            Frame frame = new Frame
            {
                time = elapsed,
                rootPositionDelta = rootDelta,
                rootRotationDelta = rootDeltaRot,
                bonePoses = CaptureBonePoses()
            };

            recordedFrames.Add(frame);
            nextSampleTime += 1f / Mathf.Max(sampleRate, 1f);
        }

        private BonePose[] CaptureBonePoses()
        {
            BonePose[] poses = new BonePose[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                Transform bone = bones[i];
                poses[i] = new BonePose
                {
                    localPosition = bone.localPosition,
                    localRotation = bone.localRotation
                };
            }

            return poses;
        }

        private void TickPlayback()
        {
            if (playbackFrames == null || playbackFrames.Length == 0)
            {
                isPlaying = false;
                return;
            }

            playbackTime += Time.deltaTime;
            while (playbackIndex < playbackFrames.Length - 1 && playbackFrames[playbackIndex + 1].time <= playbackTime)
            {
                playbackIndex++;
            }

            int nextIndex = Mathf.Min(playbackIndex + 1, playbackFrames.Length - 1);
            Frame current = playbackFrames[playbackIndex];
            Frame next = playbackFrames[nextIndex];
            float segmentDuration = Mathf.Max(next.time - current.time, Mathf.Epsilon);
            float t = Mathf.Clamp01((playbackTime - current.time) / segmentDuration);

            ApplyFrame(current, next, t);

            if (playbackIndex >= playbackFrames.Length - 1 && playbackTime >= playbackFrames[playbackFrames.Length - 1].time)
            {
                isPlaying = false;
            }
        }

        private void ApplyFrame(Frame current, Frame next, float t)
        {
            Vector3 rootDelta = Vector3.Lerp(current.rootPositionDelta, next.rootPositionDelta, t);
            Quaternion rootDeltaRot = Quaternion.Slerp(current.rootRotationDelta, next.rootRotationDelta, t);

            if (root != null)
            {
                root.position = playbackStartPosition + rootDelta;
                root.rotation = playbackStartRotation * rootDeltaRot;
            }

            for (int i = 0; i < playbackBones.Length; i++)
            {
                Transform bone = playbackBones[i];
                if (bone == null)
                {
                    continue;
                }

                BonePose currentPose = current.bonePoses[i];
                BonePose nextPose = next.bonePoses[i];
                bone.localPosition = Vector3.Lerp(currentPose.localPosition, nextPose.localPosition, t);
                bone.localRotation = Quaternion.Slerp(currentPose.localRotation, nextPose.localRotation, t);
            }
        }

        private bool LoadPlaybackData()
        {
            if (playbackJson == null)
            {
                return false;
            }

            RecordingData data = JsonUtility.FromJson<RecordingData>(playbackJson.text);
            if (data == null || data.frames == null || data.frames.Length == 0)
            {
                return false;
            }

            playbackFrames = data.frames;
            recordedBonePaths = data.bonePaths;
            return true;
        }

        private void EnsureBones()
        {
            if (bones != null && bones.Length > 0)
            {
                return;
            }

            bones = root.GetComponentsInChildren<Transform>();
        }

        private void EnsurePlaybackBones()
        {
            if (recordedBonePaths == null || recordedBonePaths.Length == 0)
            {
                playbackBones = bones;
                return;
            }

            playbackBones = new Transform[recordedBonePaths.Length];
            for (int i = 0; i < recordedBonePaths.Length; i++)
            {
                playbackBones[i] = root.Find(recordedBonePaths[i]);
            }
        }

        private string[] BuildBonePaths()
        {
            string[] paths = new string[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                paths[i] = GetPath(root, bones[i]);
            }

            return paths;
        }

        private static string GetPath(Transform rootTransform, Transform target)
        {
            if (rootTransform == target)
            {
                return string.Empty;
            }

            List<string> segments = new List<string>();
            Transform current = target;
            while (current != null && current != rootTransform)
            {
                segments.Add(current.name);
                current = current.parent;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        private static string ResolveDownloadsPath()
        {
            string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(userHome))
            {
                string downloads = Path.Combine(userHome, "Downloads");
                if (Directory.Exists(downloads))
                {
                    return downloads;
                }
            }

            return Application.persistentDataPath;
        }
    }
}

using UnityEngine;
using System;
using System.Text;
using System.Security.Cryptography;

namespace WaterSort.Core
{
    /// <summary>
    /// Security Manager - Save data encryption and anti-cheat protection
    /// CRITICAL for preventing save manipulation and cheating
    /// </summary>
    public class SecurityManager : MonoBehaviour
    {
        private static SecurityManager _instance;
        public static SecurityManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SecurityManager");
                    _instance = go.AddComponent<SecurityManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Security Settings")]
        public bool EnableEncryption = true;
        public bool EnableAntiCheat = true;
        public bool EnableTimeValidation = true;

        // IMPORTANT: Change this key for your game!
        // Use a unique, hard-to-guess key
        private const string ENCRYPTION_KEY = "Y0urS3cr3tK3y!2024#WaterSort";
        private const string HASH_SALT = "WaterSortPuzzle_SaltKey_2024";

        // Anti-cheat variables
        private DateTime _lastValidTime;
        private int _sessionSuspiciousActivity = 0;
        private const int MAX_SUSPICIOUS_ACTIONS = 5;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _lastValidTime = DateTime.Now;

            Debug.Log("SecurityManager initialized");
        }

        #region Data Encryption

        /// <summary>
        /// Encrypt string data using AES-256
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (!EnableEncryption || string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                byte[] key = DeriveKeyFromPassword(ENCRYPTION_KEY);
                byte[] iv = new byte[16]; // AES block size

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    return Convert.ToBase64String(encryptedBytes);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Encryption failed: {ex.Message}");
                return plainText;
            }
        }

        /// <summary>
        /// Decrypt string data
        /// </summary>
        public string Decrypt(string encryptedText)
        {
            if (!EnableEncryption || string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            try
            {
                byte[] key = DeriveKeyFromPassword(ENCRYPTION_KEY);
                byte[] iv = new byte[16];

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Decryption failed: {ex.Message}");
                return encryptedText;
            }
        }

        private byte[] DeriveKeyFromPassword(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes(HASH_SALT);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return deriveBytes.GetBytes(32); // 256-bit key
            }
        }

        #endregion

        #region Data Integrity

        /// <summary>
        /// Generate hash for data integrity check
        /// </summary>
        public string GenerateHash(string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string saltedData = data + HASH_SALT;
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Verify data integrity
        /// </summary>
        public bool VerifyHash(string data, string expectedHash)
        {
            string actualHash = GenerateHash(data);
            return actualHash == expectedHash;
        }

        /// <summary>
        /// Secure save - encrypt and add hash
        /// </summary>
        public void SecureSave(string key, string value)
        {
            string encryptedValue = Encrypt(value);
            string hash = GenerateHash(value);

            PlayerPrefs.SetString(key, encryptedValue);
            PlayerPrefs.SetString(key + "_hash", hash);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Secure load - decrypt and verify hash
        /// </summary>
        public string SecureLoad(string key, string defaultValue = "")
        {
            if (!PlayerPrefs.HasKey(key))
                return defaultValue;

            string encryptedValue = PlayerPrefs.GetString(key);
            string storedHash = PlayerPrefs.GetString(key + "_hash", "");

            string decryptedValue = Decrypt(encryptedValue);

            // Verify integrity
            if (!string.IsNullOrEmpty(storedHash) && !VerifyHash(decryptedValue, storedHash))
            {
                Debug.LogWarning($"Data integrity check failed for key: {key}");
                OnSuspiciousActivity("Data tampering detected");
                return defaultValue;
            }

            return decryptedValue;
        }

        /// <summary>
        /// Secure save for int
        /// </summary>
        public void SecureSaveInt(string key, int value)
        {
            SecureSave(key, value.ToString());
        }

        /// <summary>
        /// Secure load for int
        /// </summary>
        public int SecureLoadInt(string key, int defaultValue = 0)
        {
            string value = SecureLoad(key, defaultValue.ToString());
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        #endregion

        #region Anti-Cheat

        /// <summary>
        /// Validate if value change is legitimate
        /// </summary>
        public bool ValidateValueChange(int oldValue, int newValue, int maxIncrease)
        {
            if (!EnableAntiCheat)
                return true;

            int change = newValue - oldValue;

            // Check for impossible increases
            if (change > maxIncrease)
            {
                Debug.LogWarning($"Suspicious value change detected: {oldValue} -> {newValue} (max: {maxIncrease})");
                OnSuspiciousActivity("Impossible value increase");
                return false;
            }

            // Check for negative progression (except undo/spend)
            if (change < 0 && oldValue < newValue)
            {
                Debug.LogWarning($"Invalid value manipulation: {oldValue} -> {newValue}");
                OnSuspiciousActivity("Invalid progression");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Time-based validation to prevent time manipulation
        /// </summary>
        public bool ValidateTimeProgression()
        {
            if (!EnableTimeValidation)
                return true;

            DateTime currentTime = DateTime.Now;
            TimeSpan timeDiff = currentTime - _lastValidTime;

            // Check for time travel (backward)
            if (timeDiff.TotalSeconds < -60) // Allow 1 minute tolerance
            {
                Debug.LogWarning("Time manipulation detected (backward)");
                OnSuspiciousActivity("Time manipulation");
                return false;
            }

            // Check for impossible time jump (forward)
            if (timeDiff.TotalHours > 24)
            {
                Debug.LogWarning("Suspicious time jump detected");
                OnSuspiciousActivity("Time jump");
                return false;
            }

            _lastValidTime = currentTime;
            return true;
        }

        /// <summary>
        /// Check if player is trying to cheat with memory editing
        /// </summary>
        public bool DetectMemoryManipulation(int storedValue, int observedValue)
        {
            if (!EnableAntiCheat)
                return false;

            if (storedValue != observedValue)
            {
                Debug.LogWarning($"Memory manipulation detected: stored={storedValue}, observed={observedValue}");
                OnSuspiciousActivity("Memory editing");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Obfuscate integer value to prevent memory editing
        /// </summary>
        public int ObfuscateInt(int value)
        {
            // Simple XOR obfuscation - can be enhanced
            int key = 0x5A5A5A5A;
            return value ^ key;
        }

        /// <summary>
        /// De-obfuscate integer value
        /// </summary>
        public int DeobfuscateInt(int obfuscatedValue)
        {
            int key = 0x5A5A5A5A;
            return obfuscatedValue ^ key;
        }

        private void OnSuspiciousActivity(string reason)
        {
            _sessionSuspiciousActivity++;

            AnalyticsManager.Instance?.LogEvent("suspicious_activity", new System.Collections.Generic.Dictionary<string, object>
            {
                { "reason", reason },
                { "count", _sessionSuspiciousActivity }
            });

            if (_sessionSuspiciousActivity >= MAX_SUSPICIOUS_ACTIONS)
            {
                // Take action - ban, reset progress, etc.
                Debug.LogError("Maximum suspicious activity reached. Taking action...");
                OnCheatDetected();
            }
        }

        private void OnCheatDetected()
        {
            // Implement your cheat response here
            // Options:
            // 1. Reset player progress
            // 2. Ban player (if you have server)
            // 3. Show warning message
            // 4. Disable certain features

            Debug.LogError("CHEAT DETECTED - Taking appropriate action");

            // Example: Reset suspicious values
            // SaveManager.Instance.ResetAllProgress();

            // Show warning to player
            // UIManager.Instance.ShowMessage("Suspicious activity detected");
        }

        #endregion

        #region Secure Random

        /// <summary>
        /// Cryptographically secure random number generator
        /// Use for important random values (rewards, drops, etc.)
        /// </summary>
        public int SecureRandomRange(int min, int max)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomNumber = new byte[4];
                rng.GetBytes(randomNumber);

                int value = BitConverter.ToInt32(randomNumber, 0);
                int range = max - min;

                return min + (Math.Abs(value) % range);
            }
        }

        #endregion

        #region Device Fingerprint

        /// <summary>
        /// Generate unique device fingerprint for player identification
        /// </summary>
        public string GetDeviceFingerprint()
        {
            string fingerprint = $"{SystemInfo.deviceUniqueIdentifier}_{SystemInfo.deviceModel}_{SystemInfo.operatingSystem}";
            return GenerateHash(fingerprint);
        }

        /// <summary>
        /// Check if device fingerprint matches saved one
        /// </summary>
        public bool ValidateDeviceFingerprint()
        {
            string currentFingerprint = GetDeviceFingerprint();
            string savedFingerprint = PlayerPrefs.GetString("device_fingerprint", "");

            if (string.IsNullOrEmpty(savedFingerprint))
            {
                // First time - save it
                PlayerPrefs.SetString("device_fingerprint", currentFingerprint);
                PlayerPrefs.Save();
                return true;
            }

            if (currentFingerprint != savedFingerprint)
            {
                Debug.LogWarning("Device fingerprint mismatch - possible save file transfer");
                OnSuspiciousActivity("Device mismatch");
                return false;
            }

            return true;
        }

        #endregion
    }
}

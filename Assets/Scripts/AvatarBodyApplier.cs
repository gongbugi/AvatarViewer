using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UMA.CharacterSystem;
using System.Runtime.InteropServices;

[Serializable]
public class AvatarBodyData
{
    [Header("신체 치수 (0-1 범위)")]
    public float uma_height = 0.5f;       // 키
    public float uma_belly = 0.5f;        // 배
    public float uma_waist = 0.5f;        // 허리
    public float uma_width = 0.5f;        // 어깨 너비
    public float uma_fore_arm = 0.5f;     // 팔뚝 너비
    public float uma_arm = 0.5f;          // 팔 길이
    public float uma_legs = 0.5f;         // 다리 크기
}

public class AvatarBodyApplier : MonoBehaviour
{
    [Header("설정")]
    public string jsonFileName = "body_measurements.json";
    public bool useLocalFile = false;
    public string serverUrl = "http://15.165.129.131:3000/api/mannequin/showMannequin";
    
    [Header("UMA 아바타 참조")]
    public DynamicCharacterAvatar avatar;
    
    private AvatarBodyData currentBodyData;
    private string authToken = "";
    
    public event Action<AvatarBodyData> OnBodyDataLoaded;
    public event Action<string> OnErrorOccurred;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetTokenFromLocalStorage();
    
    [DllImport("__Internal")]
    private static extern void SetTokenToLocalStorage(string token);
    
    [DllImport("__Internal")]
    private static extern void RemoveTokenFromLocalStorage();
#endif

    void Start()
    {
        if (avatar == null)
        {
            avatar = FindFirstObjectByType<DynamicCharacterAvatar>();
            if (avatar == null)
            {
                Debug.LogError("씬에서 DynamicCharacterAvatar를 찾을 수 없습니다!");
                OnErrorOccurred?.Invoke("UMA 아바타를 찾을 수 없습니다.");
                return;
            }
        }
        
        // LocalStorage에서 토큰 추출
        ExtractTokenFromLocalStorage();
        
        LoadBodyData();
    }

    /// <summary>
    /// LocalStorage에서 토큰 추출
    /// </summary>
    private void ExtractTokenFromLocalStorage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            string token = GetTokenFromLocalStorage();
            
            if (!string.IsNullOrEmpty(token))
            {
                authToken = token;
                Debug.Log("LocalStorage에서 토큰 추출 성공");
                
                // 토큰을 PlayerPrefs에도 저장 (백업용)
                PlayerPrefs.SetString("AuthToken", authToken);
                PlayerPrefs.Save();
            }
            else
            {
                // LocalStorage에 토큰이 없으면 PlayerPrefs에서 시도
                authToken = PlayerPrefs.GetString("AuthToken", "");
                if (!string.IsNullOrEmpty(authToken))
                {
                    Debug.Log("PlayerPrefs에서 저장된 토큰 사용");
                }
                else
                {
                    Debug.LogWarning("토큰을 찾을 수 없습니다. 로그인 페이지에서 토큰이 LocalStorage에 저장되었는지 확인해주세요.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("LocalStorage에서 토큰 추출 중 오류: " + e.Message);
            // 오류 시 저장된 토큰 시도
            authToken = PlayerPrefs.GetString("AuthToken", "");
        }
#else
        // 에디터나 다른 플랫폼에서는 저장된 토큰 사용
        authToken = PlayerPrefs.GetString("AuthToken", "");
        Debug.Log("에디터 모드: 저장된 토큰 사용");
#endif
    }

    /// <summary>
    /// JSON 파일에서 신체 데이터 로드
    /// </summary>
    public void LoadBodyData()
    {
        if (useLocalFile)
        {
            StartCoroutine(LoadBodyDataFromLocalFile());
        }
        else
        {
            StartCoroutine(LoadBodyDataFromServer());
        }
    }

    private IEnumerator LoadBodyDataFromLocalFile()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, jsonFileName);
        
        UnityWebRequest request = UnityWebRequest.Get(filePath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonContent = request.downloadHandler.text;
            ProcessBodyData(jsonContent);
        }
        else
        {
            string error = $"신체 데이터 JSON 파일 로드 실패: {request.error}";
            Debug.LogError(error);
            OnErrorOccurred?.Invoke(error);
        }

        request.Dispose();
    }

    private IEnumerator LoadBodyDataFromServer()
    {
        UnityWebRequest request = UnityWebRequest.Get(serverUrl);
        
        // 토큰이 있으면 Authorization 헤더에 추가
        if (!string.IsNullOrEmpty(authToken))
        {
            request.SetRequestHeader("Authorization", "Bearer " + authToken);
            Debug.Log("Authorization 헤더 추가됨");
        }
        else
        {
            Debug.LogWarning("토큰이 없습니다. 401 오류가 발생할 수 있습니다.");
        }
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonContent = request.downloadHandler.text;
            Debug.Log("서버에서 데이터 로드 성공");
            ProcessBodyData(jsonContent);
        }
        else
        {
            string error = $"서버에서 신체 데이터 로드 실패: {request.error}";
            
            // 401 오류인 경우 특별 처리
            if (request.responseCode == 401)
            {
                error += "\n토큰이 유효하지 않거나 만료되었습니다. URL에 ?token=your_token_here를 추가해주세요.";
            }
            
            Debug.LogError(error);
            Debug.LogError($"응답 코드: {request.responseCode}");
            OnErrorOccurred?.Invoke(error);
        }

        request.Dispose();
    }

    private void ProcessBodyData(string jsonContent)
    {
        try
        {
            currentBodyData = JsonUtility.FromJson<AvatarBodyData>(jsonContent);
            Debug.Log("신체 데이터 로드 완료");
            Debug.Log($"Height: {currentBodyData.uma_height}, Belly: {currentBodyData.uma_belly}, Waist: {currentBodyData.uma_waist}");
            
            OnBodyDataLoaded?.Invoke(currentBodyData);
            ApplyBodyDataToAvatar();
        }
        catch (Exception e)
        {
            string error = $"신체 데이터 JSON 파싱 오류: {e.Message}";
            Debug.LogError(error);
            OnErrorOccurred?.Invoke(error);
        }
    }

    /// <summary>
    /// 로드된 신체 데이터를 UMA 아바타에 직접 적용
    /// </summary>
    public void ApplyBodyDataToAvatar()
    {
        if (avatar == null)
        {
            Debug.LogError("UMA 아바타가 설정되지 않았습니다!");
            return;
        }

        if (currentBodyData == null)
        {
            Debug.LogError("신체 데이터가 로드되지 않았습니다!");
            return;
        }

        try
        {
            // UMA DNA 값 적용 (0-1 범위)
            avatar.SetDNA("height", currentBodyData.uma_height);       // 키
            avatar.SetDNA("belly", currentBodyData.uma_belly);         // 배
            avatar.SetDNA("waist", currentBodyData.uma_waist);         // 허리
            avatar.SetDNA("armWidth", currentBodyData.uma_width);      // 어깨 너비
            avatar.SetDNA("forearmWidth", currentBodyData.uma_fore_arm); // 팔뚝 너비
            avatar.SetDNA("armLength", currentBodyData.uma_arm);       // 팔 길이
            avatar.SetDNA("legsSize", currentBodyData.uma_legs);        // 다리 크기

            // 아바타 리빌드
            avatar.BuildCharacter();
            
            Debug.Log("UMA 아바타에 신체 데이터 적용 완료!");
            Debug.Log($"적용된 값 - Height: {currentBodyData.uma_height:F3}, Belly: {currentBodyData.uma_belly:F3}, Waist: {currentBodyData.uma_waist:F3}, Width: {currentBodyData.uma_width:F3}, ForeArm: {currentBodyData.uma_fore_arm:F3}, Arm: {currentBodyData.uma_arm:F3}, Legs: {currentBodyData.uma_legs:F3}");
        }
        catch (Exception e)
        {
            string error = $"UMA DNA 적용 중 오류 발생: {e.Message}";
            Debug.LogError(error);
            OnErrorOccurred?.Invoke(error);
        }
    }

    /// <summary>
    /// 직접 신체 데이터를 설정하고 적용
    /// </summary>
    public void SetBodyData(AvatarBodyData bodyData)
    {
        currentBodyData = bodyData;
        ApplyBodyDataToAvatar();
    }

    /// <summary>
    /// 개별 신체 파라미터 설정
    /// </summary>
    public void SetHeight(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_height = Mathf.Clamp01(value);
        ApplyIndividualDNA("height", currentBodyData.uma_height);
    }

    public void SetBelly(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_belly = Mathf.Clamp01(value);
        ApplyIndividualDNA("belly", currentBodyData.uma_belly);
    }

    public void SetWaist(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_waist = Mathf.Clamp01(value);
        ApplyIndividualDNA("waist", currentBodyData.uma_waist);
    }

    public void SetWidth(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_width = Mathf.Clamp01(value);
        ApplyIndividualDNA("armWidth", currentBodyData.uma_width);
    }

    public void SetForeArm(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_fore_arm = Mathf.Clamp01(value);
        ApplyIndividualDNA("forearmWidth", currentBodyData.uma_fore_arm);
    }

    public void SetArm(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_arm = Mathf.Clamp01(value);
        ApplyIndividualDNA("armLength", currentBodyData.uma_arm);
    }

    public void SetLegs(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_legs = Mathf.Clamp01(value);
        ApplyIndividualDNA("legsSize", currentBodyData.uma_legs);
    }

    /// <summary>
    /// 개별 DNA 값 적용
    /// </summary>
    private void ApplyIndividualDNA(string dnaName, float value)
    {
        if (avatar == null) return;

        try
        {
            avatar.SetDNA(dnaName, value);
            avatar.BuildCharacter();
            Debug.Log($"{dnaName} DNA 값을 {value:F3}로 설정했습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"{dnaName} DNA 적용 중 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 아바타를 기본 체형으로 초기화
    /// </summary>
    public void ResetToDefault()
    {
        if (avatar == null) return;

        try
        {
            avatar.SetDNA("height", 0.5f);
            avatar.SetDNA("belly", 0.5f);
            avatar.SetDNA("waist", 0.5f);
            avatar.SetDNA("armWidth", 0.5f);
            avatar.SetDNA("forearmWidth", 0.5f);
            avatar.SetDNA("armLength", 0.5f);
            avatar.SetDNA("legsSize", 0.5f);

            avatar.BuildCharacter();
            
            currentBodyData = new AvatarBodyData();
            Debug.Log("아바타를 기본 체형으로 초기화했습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"기본 체형 초기화 중 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 현재 신체 데이터 반환
    /// </summary>
    public AvatarBodyData GetCurrentBodyData()
    {
        return currentBodyData;
    }

    /// <summary>
    /// 현재 신체 데이터를 JSON 문자열로 반환
    /// </summary>
    public string GetBodyDataAsJson()
    {
        if (currentBodyData == null) return null;
        return JsonUtility.ToJson(currentBodyData, true);
    }

    /// <summary>
    /// JSON 문자열에서 신체 데이터 설정
    /// </summary>
    public void SetBodyDataFromJson(string jsonString)
    {
        try
        {
            AvatarBodyData bodyData = JsonUtility.FromJson<AvatarBodyData>(jsonString);
            SetBodyData(bodyData);
        }
        catch (Exception e)
        {
            string error = $"JSON에서 신체 데이터 설정 중 오류: {e.Message}";
            Debug.LogError(error);
            OnErrorOccurred?.Invoke(error);
        }
    }

    /// <summary>
    /// 외부에서 토큰 설정 (테스트용)
    /// </summary>
    public void SetAuthToken(string token)
    {
        authToken = token;
        PlayerPrefs.SetString("AuthToken", authToken);
        PlayerPrefs.Save();
        
#if UNITY_WEBGL && !UNITY_EDITOR
        // LocalStorage에도 저장
        SetTokenToLocalStorage(token);
#endif
        
        Debug.Log("토큰이 수동으로 설정되었습니다.");
    }

    /// <summary>
    /// 현재 토큰 상태 확인
    /// </summary>
    public string GetCurrentToken()
    {
        return authToken;
    }
}
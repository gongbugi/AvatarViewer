using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UMA.CharacterSystem;

[Serializable]
public class AvatarBodyData
{
    [Header("신체 치수 (0-1 범위)")]
    public float uma_height = 0.5f;       // 키
    public float uma_width = 0.5f;        // 어깨 너비 (원래 shoulder_width)
    public float uma_waist = 0.5f;        // 허리
    public float uma_belly = 0.5f;        // 배
    public float uma_fore_arm = 0.5f;     // 팔뚝 길이
    public float uma_arm = 0.5f;          // 팔 길이
    public float uma_legs = 0.5f;         // 다리 길이
}

public class AvatarBodyApplier : MonoBehaviour
{
    [Header("설정")]
    public string jsonFileName = "body_measurements.json";
    public bool useLocalFile = true;
    public string serverUrl = "https://yourserver.com/api/measurements";
    
    [Header("UMA 아바타 참조")]
    public DynamicCharacterAvatar avatar;
    
    private AvatarBodyData currentBodyData;
    
    public event Action<AvatarBodyData> OnBodyDataLoaded;
    public event Action<string> OnErrorOccurred;

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
        
        LoadBodyData();
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
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonContent = request.downloadHandler.text;
            ProcessBodyData(jsonContent);
        }
        else
        {
            string error = $"서버에서 신체 데이터 로드 실패: {request.error}";
            Debug.LogError(error);
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
            Debug.Log($"Height: {currentBodyData.uma_height}, Width: {currentBodyData.uma_width}, Waist: {currentBodyData.uma_waist}");
            
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
            avatar.SetDNA("armWidth", currentBodyData.uma_width);      // 어깨 너비 (원래 shoulder_width)
            avatar.SetDNA("waist", currentBodyData.uma_waist);         // 허리
            avatar.SetDNA("belly", currentBodyData.uma_belly);         // 배
            avatar.SetDNA("forearmLength", currentBodyData.uma_fore_arm); // 팔뚝 길이
            avatar.SetDNA("armLength", currentBodyData.uma_arm);       // 팔 길이
            avatar.SetDNA("legsSize", currentBodyData.uma_legs);  // 다리 길이

            // 아바타 리빌드
            avatar.BuildCharacter();
            
            Debug.Log("UMA 아바타에 신체 데이터 적용 완료!");
            Debug.Log($"적용된 값 - Height: {currentBodyData.uma_height:F3}, Width: {currentBodyData.uma_width:F3}, Waist: {currentBodyData.uma_waist:F3}, Belly: {currentBodyData.uma_belly:F3}, ForeArm: {currentBodyData.uma_fore_arm:F3}, Arm: {currentBodyData.uma_arm:F3}, Legs: {currentBodyData.uma_legs:F3}");
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

    public void SetWidth(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_width = Mathf.Clamp01(value);
        ApplyIndividualDNA("armWidth", currentBodyData.uma_width);
    }

    public void SetWaist(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_waist = Mathf.Clamp01(value);
        ApplyIndividualDNA("waist", currentBodyData.uma_waist);
    }

    public void SetBelly(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_belly = Mathf.Clamp01(value);
        ApplyIndividualDNA("belly", currentBodyData.uma_belly);
    }

    public void SetForeArm(float value)
    {
        if (currentBodyData == null) currentBodyData = new AvatarBodyData();
        currentBodyData.uma_fore_arm = Mathf.Clamp01(value);
        ApplyIndividualDNA("forearmLength", currentBodyData.uma_fore_arm);
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
            avatar.SetDNA("armWidth", 0.5f);
            avatar.SetDNA("waist", 0.5f);
            avatar.SetDNA("belly", 0.5f);
            avatar.SetDNA("forearmLength", 0.5f);
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
}
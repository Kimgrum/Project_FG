using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FGDefine;

public class PlayerCamera : MonoBehaviour
{
    public Camera cam;
    public Transform target;

    public float halfHeight;
    public float halfWidth;

    public float clampedX;
    public float clampedY;

    public Vector2 minBound;
    public Vector2 maxBound;
    public Vector3 mapCenterPos;

    float mapSize;
    float playerCamSize;
    float zoomSpeed = 0.1f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        FollowingCamera();
    }

    public void Init(BaseMap _map)
    {
        cam = Camera.main;

        Set_MapTransform(_map.transform.position);
        Set_CameraBounds(_map.maxBound, _map.minBound);
        Set_Target(_map.transform);

        playerCamSize = 5f;
        mapSize = _map.maxBound.x * Screen.height / Screen.width;

        Set_OrthographicSize(mapSize);

        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * Screen.width / Screen.height;
    }

    public void Following_Target(Transform target)
    {
        Set_Target(target);
        Set_ZoomIn();
    }

    public void Set_CameraBounds(Vector2 _maxBound, Vector2 _minBound)
    {
        minBound = _minBound;
        maxBound = _maxBound;
    }

    private void FollowingCamera()
    {
        clampedX = Mathf.Clamp(target.transform.position.x, minBound.x + halfWidth, maxBound.x - halfWidth);
        clampedY = Mathf.Clamp(target.transform.position.y, minBound.y + halfHeight, maxBound.y - halfHeight);

        transform.position = new Vector3(clampedX, clampedY, -10);
    }

    public void Set_ZoomOut()
    {
        if (this.mapSize > cam.orthographicSize)
            StartCoroutine(ICamera_ZoomOut(zoomSpeed));
    }

    public void Set_ZoomIn()
    {
        if (this.playerCamSize < cam.orthographicSize)
            StartCoroutine(ICamera_ZoomIn(zoomSpeed * -1f));
    }

    // 맵의 위치값
    public void Set_MapTransform(Vector3 _position)
    {
        this.mapCenterPos = _position;
        this.mapCenterPos.z = this.transform.position.z;
    }

    public void Set_Target(Transform _transform)
    {
        this.target = _transform;
    }

    public void Set_OrthographicSize(float _size)
    {
        cam.orthographicSize = _size;
    }
    
    IEnumerator ICamera_ZoomIn(float _zoomSpeed)
    {
        while (cam.orthographicSize > playerCamSize)
        {
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * Screen.width / Screen.height;

            cam.orthographicSize += _zoomSpeed;
            Set_OrthographicSize(cam.orthographicSize);
            yield return new WaitForSeconds(0.01f);
        }

        Set_OrthographicSize(playerCamSize);
    }

    IEnumerator ICamera_ZoomOut(float _zoomSpeed)
    {
        float zoomCount = (mapSize - cam.orthographicSize) / zoomSpeed;
        float DistinceDelta = Vector3.Distance(transform.position, mapCenterPos) / zoomCount;

        while (cam.orthographicSize < mapSize)
        {
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * Screen.width / Screen.height;

            transform.position = Vector3.MoveTowards(transform.position, mapCenterPos, DistinceDelta);
            cam.orthographicSize += _zoomSpeed;
            Set_OrthographicSize(cam.orthographicSize);
            yield return new WaitForSeconds(0.01f);
        }

        Set_OrthographicSize(mapSize);
    }
}

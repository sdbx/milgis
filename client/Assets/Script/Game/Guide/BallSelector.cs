﻿using System.Collections;
using System.Collections.Generic;
using Game.Balls;
using Game.Boards;
using Game.Coords;
using Models;
using UnityEngine;

namespace Game.Guide
{
    public class BallSelector
    {
        public bool _isSelected = false;

        private BallManager _ballManager;
        private BoardManager _boardManager;

        private bool _isFirstBallpicked = false;
        private CubeCoord _firstBallSelection, _lastBallSelection;


        public BallSelector(BallManager ballManager, BoardManager boardManager)
        {
            _ballManager = ballManager;
            _boardManager = boardManager;
        }

        public BallSelection GetBallSelection()
        {
            return new BallSelection(_firstBallSelection, _lastBallSelection);
        }

        public CubeCoord GetBallCubeCoordByMouseXY(Vector2 mouseXY)
        {
            int s = _boardManager.GetBoard().GetSide() - 1;
            for (int x = 0; x <= 2 * s; x++)
            {
                for (int y = 0; y <= 2 * s; y++)
                {
                    Ball ballObject = _ballManager.ballObjects[x, y];
                    if (ballObject == null)
                        continue;

                    if (Utils.CheckCircleAndPointCollision(mouseXY, ballObject.transform.position, _ballManager.sizeOfBall / 2))
                    {
                        x = x - s;
                        y = y - s;
                        return new CubeCoord(x, y, -(x + y));
                    }

                }
            }
            return null;
        }

        public void SelectingBalls(BallType ballType,SelectGuideDisplay guide,ZoomSystem zoomSystem)
        {
            if (_isSelected)
            {
                return;
            }
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            CubeCoord ballCubeCoord = GetBallCubeCoordByMouseXY(mousePos);


            if(_isFirstBallpicked)
            {
                zoomSystem.isLocked = true;
                guide.SetPoint1(_ballManager.GetBallByCubeCoord(_firstBallSelection).transform.position);
                guide.SetPoint2(mousePos);
            }

            if (ballCubeCoord == null)
            {
                if(!_isFirstBallpicked)
                {
                    zoomSystem.isLocked = false;
                    guide.Hide();
                }
                UnSelectable(guide);
                return;
            }
            Ball ballObject = _ballManager.GetBallObjectByCubeCoord(ballCubeCoord);
            if (ballObject.GetBall() != ballType)
                return;

            int s = _boardManager.GetBoard().GetSide() - 1;



            if (!_isFirstBallpicked)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _firstBallSelection = ballCubeCoord;
                    _isFirstBallpicked = true;
                }
                else
                {
                    guide.displayChanger.SetMode("On");
                    guide.SetPoint1(_ballManager.GetBallByCubeCoord(ballCubeCoord).transform.position);
                    guide.SetPoint2(_ballManager.GetBallByCubeCoord(ballCubeCoord).transform.position);
                    return;
                }
            }
            else
            {
                //라인 시작점
                if (ballCubeCoord.CheckInSameLine(_firstBallSelection))
                {
                    int distance = Utils.GetDistanceBy2CubeCoord(ballCubeCoord, _firstBallSelection);
                    
                    if (distance == 0)
                    {
                        _lastBallSelection = ballCubeCoord;
                        Selectable(distance,guide);
                        guide.SetPoint2(_ballManager.GetBallByCubeCoord(_lastBallSelection).transform.position);
                        return;
                    }
                    else if (distance == 1)
                    {
                        _lastBallSelection = ballCubeCoord;
                        Selectable(distance,guide);
                        guide.SetPoint2(_ballManager.GetBallByCubeCoord(_lastBallSelection).transform.position);
                        return;
                    }
                    else if (distance == 2)
                    {
                        Ball middleBall = _ballManager.GetBallObjectByCubeCoord(
                            CubeCoord.GetCenter(ballCubeCoord, _firstBallSelection));

                        if (middleBall != null && _boardManager.CheckBallObjectIsMine(middleBall.gameObject))
                        {
                            _lastBallSelection = ballCubeCoord;
                            Selectable(distance,guide);
                            guide.SetPoint2(_ballManager.GetBallByCubeCoord(_lastBallSelection).transform.position);
                            return;
                        }
                    }
                    
                }
            }
            UnSelectable(guide);
        }

        private void UnSelectable(SelectGuideDisplay guide)
        {
            if (Input.GetMouseButtonUp(0))
            {
                _isFirstBallpicked = false;
            }
            else
            {
                guide.displayChanger.SetMode("Selecting");
            }
        }

        private void Selectable(int count, SelectGuideDisplay guide)
        {
            if (Input.GetMouseButtonUp(0))
            {
                _isSelected = true;
                _isFirstBallpicked = false;
            }
            else
            {
                guide.displayChanger.SetMode("CanSelect");
            }
        }

    }

}
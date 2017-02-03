﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MultiNetAgent : NEAgent
{
    private MultiNetManager manager;
    private int viewingIndex = -1;
    private string coinName = "";
    private bool doNotTickOnce = false;

    protected override void Awake()
    {
        //manager = new MultiNetManager(this);
        //manager = new OptimizingMultiNetManager(this);
        manager = new LinearMultiNetManager(this, timeKeep);

        base.Awake();
    }

    protected void Start()
    {

    }

    protected override void BeginLevel()
    {
        manager.BeginLevel();
        viewingIndex = -1;
        coinName = "";
        doNotTickOnce = false;

        InitialSettings();
        DetermineViewing();
        setLearningText();
    }

    protected override void DetermineViewing()
    {
        if (loadPath.Length > 0)
        {
            manager.LoadNets(loadPath);
            timeKeep.forceView = true;
            timeKeep.BeginLevel();
            timeKeep.forceView = true;
        }

        if (timeKeep.GetViewing())
        {
            viewingIndex = 0;
            if (manager.bestCount() > 0)
            {
                net = manager.getBestNet(0);
            }
            else
                net = new NeuralNet();
        }
        else
        {
            net = new NeuralNet();
            if (manager.netIndex > 0)
            {
                setScenario();
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (!doNotTickOnce && !stopTick && tickDone)
            tick();
        else
            doNotTickOnce = false;
    }
    
    /*protected override void Update()
    {
        base.Update();
        if (!doNotTickOnce && !stopTick && tickDone)
            tick();
        else
            doNotTickOnce = false;
    }*/

    protected override void setLearningText()
    {
        learningText.fontSize = 16;
        learningText.text = "";
        learningText2.text = "";
        learningText3.text = "";
        String[] line = { "", "", "", "" };

        if (timeKeep.GetViewing())
        {
            line[0] = "Viewing Elite Nets";
            for (int i = 0; i < manager.bestCount(); i++)
            {
                if ((i == viewingIndex))
                {
                    line[1] += "<color=#ff0000>" + manager.getBestCoinName(i) + "</color>\t\t";
                    line[2] += "<color=#ff0000>" + (manager.getBestFitness(i) > 0 ? "  "  : " ") +
                        manager.getBestFitness(i).ToString("F2") + "</color>\t\t\t";
                }
                else
                {
                    line[1] += manager.getBestCoinName(i) + "\t\t";
                    line[2] += "  " + manager.getBestFitness(i).ToString("F2") + "\t\t\t";
                }
            }
        }
        else
        {
            for (int i = 0; i <= manager.count(); i++)
            {
                if ((i == manager.netIndex))
                {
                    line[1] += "<color=#ff0000>" + manager.getCoinName(i) + "</color>\t\t";
                    line[2] += "<color=#ff0000>" + (manager.getBestFitness(i) > 0 ? "  " : " ") +
                        manager.getBestFitness(i).ToString("F2") + "</color>\t\t\t";
                    line[3] += "<color=#ff0000>" + manager.evaluations + "/" + manager.getMaxEvaluations() + "</color>\t\t";
                }
                else
                {
                    line[1] += manager.getCoinName(i) + "\t\t";
                    line[2] += "  " + manager.getFitness(i).ToString("F2") + "\t\t\t";
                    line[3] += "            \t\t";
                }
            }

        }

        for (int i = 0; i < 4; i++)
        {
            learningText.text += line[i] + "\n";
        }
    }

    public void setScenario()
    {
        transform.position = manager.getPosition(manager.netIndex - 1);
        velocityX = manager.getVelocityX(manager.netIndex - 1);
        velocityY = manager.getVelocityY(manager.netIndex - 1);
        facingRight = manager.getFacingRight(manager.netIndex - 1);
        jump = manager.getJump(manager.netIndex - 1);
        grounded = manager.getGrounded(manager.netIndex - 1);
        manager.destroyCoins();
    }

    public override void grabCoin(string coinName)
    {
        if (timeKeep.GetViewing())
            base.grabCoin(coinName);
        this.coinName = coinName;
        if (!timeKeep.GetViewing())
        {
            stopTick = true;
            LevelEnd();
        }
        else
        {
            if (viewingIndex < (manager.bestCount() - 1))
            {
                if (loadPath.Length == 0)
                    manager.LogScenario(OutputCurrentScenario(), viewingIndex);
                viewingIndex += 1;
                net = manager.getBestNet(viewingIndex);
                doNotTickOnce = true;
                setLearningText();
            }
        }
    }

    protected string OutputCurrentScenario()
    {
        return coinName + ",  " + transform.position + ", " + velocityX + ", " + velocityY + ", " + facingRight + ", " + jump + ", " + grounded;
    }

    protected override void submitScore()
    {
        if (coinName.Length > 0)
        {
            ArrayList scenario = new ArrayList();
            scenario.Add(net);
            scenario.Add(coinName);
            scenario.Add(score);
            scenario.Add(transform.position);
            scenario.Add(velocityX);
            scenario.Add(velocityY);
            scenario.Add(facingRight);
            scenario.Add(jump);
            scenario.Add(grounded);

            stopTick = true;
            manager.submitNetScore(score, scenario);
        }
    }

    public override void grabWin()
    {
        base.grabWin();
        if (!timeKeep.GetViewing())
            coinName = "WinTrigger";
    }

    public override void LevelEnd()
    {
        if (timeKeep.GetViewing())
        {
            if (viewingIndex != (manager.bestCount() - 1))
            {
                if (loadPath.Length == 0)
                {
                    manager.OutputLog();
                    manager.WriteNets("" + manager.GetBestScore());
                }
                Debug.Log(timeKeep.getRestart());
            }
            if (loadPath.Length == 0)
                manager.ClearLog();
        }
        base.LevelEnd();
    }
}
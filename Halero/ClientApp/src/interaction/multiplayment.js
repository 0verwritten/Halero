import React from 'react';

export class MultiplayerPlayment extends React.Component{
    constructor( props ){
        super(props);
    }

    render(){
        require('./css/game.css');
        return (
            <div class="gamePlate singleUser">
                <div class="userPlate yours">
                    <div class="profileName">
                        Person
                    </div>
                    <div class="score">
                        Score: <span>666</span>
                    </div>
                </div>
                <canvas class="gamePlayCanvas"></canvas>
                <div class="userPlate opponent hidden">
                    <div class="profileName">
                        Person
                    </div>
                    <div class="score">
                        Score: <span>0</span>
                    </div>
                </div>
            </div>
        );
    }
}
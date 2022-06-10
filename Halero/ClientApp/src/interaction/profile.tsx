import React from 'react';
import { removeCookie } from 'typescript-cookie';
import { UserProfileData } from '../models/user-profile-model';


export class UserProfile extends React.Component< {}, UserProfileData>{
    constructor(props: {}){
        super(props);

        this.state = {
            userName: "",
            profileName: "",
            pictureUri: ""
        };

        this.updateInfo();
    }

    updateInfo(){
        fetch("/api/Authentication/ProfileInfo", { 
            method: "POST", 
            headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
            }}).then( resp => { 
                if (resp.status === 200)
                    return resp.json();
                
                removeCookie("accessToken", { path: "/" });
                removeCookie("refreshToken", { path: "/" }); 
                window.location.reload();
                }).then( (data: UserProfileData) => {
                this.setState( data );
            });
    }

    render(){
        require('./css/profile.css');
        return (
            <div className="userProfello">
                <div className="visitCard">
                    <img src={ this.state.pictureUri } className="profilePic" alt="usery" />
                    <div className="cardData">
                        <span className="profileNamo">
                            { this.state.profileName }
                        </span>
                        <span className="userName">
                            { this.state.userName }
                        </span>
                    </div>
                </div>
                <div className="generalization">
                    <div className="commentBase">
                        <div className="commentBlock">
                            <div className="commentInstance">
                                <img src="https://www.gravatar.com/avatar/205e460b479e2e5b48aec07710c08d50" alt="beau" className="userPic" />
                                <span className="commentText">
                                    A a little addicted to that staff
                                </span>
                                <button className="deleteComment">
                                </button>
                            </div>
                        </div>
                        <div className="commentPostBar">
                            <input type="text" className="commentInputBox" placeholder="Enter comment" />
                            <input type="submit" className="commentPost" value="" />
                        </div>
                    </div>
                    <div className="gamesHistory">
                        <div className="historyInstance">
                            <span className="gameName">
                                Solo game
                            </span>
                            <span className="result snakeVictory"></span>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}
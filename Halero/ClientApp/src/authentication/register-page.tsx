import React from 'react';
import { toast } from 'react-toastify';
import { AuthenticationForm } from "../models/authentication-models";

type RegistroStates = {
    userName:       string;
    profileName:    string;
    email:          string;
    password:       string;
}

export class Registro extends React.Component<{}, RegistroStates>{

    handleSubmit(e: React.FormEvent<HTMLFormElement>){
        e.preventDefault();

        fetch("/api/authentication/Register", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify( this.state ) 
          }).then( data => data.json() )
            .then( ( data: AuthenticationForm ) => {
                if(data.errors != null){
                    data.errors.forEach( error => toast.error(error) );
                }else{
                    toast.success("You've registered login to start playing");
                }
            });
    }

    render(){
        return (
            <form className="logini" onSubmit={ e => this.handleSubmit(e) } action="/api/authentication/Register" method='POST'>
                <div className="inputoBorder">
                    <input className="inputo" placeholder="Username" type="text" name="userName" minLength={ 3 } maxLength={ 30 } required 
                        onInput={ (e) => this.setState( { userName: e.currentTarget.value } ) } />
                </div>
                <div className="inputoBorder">
                    <input className="inputo" placeholder="Profile name" type="text" name="profileName" maxLength={ 30 } required 
                        onInput={ (e) => this.setState( { profileName: e.currentTarget.value } ) } />
                </div>
                <div className="inputoBorder">
                    <input className="inputo" placeholder="Email" type="email" name="email" required 
                        onInput={ (e) => this.setState( { email: e.currentTarget.value } ) } />
                </div>
                <div className="inputoBorder">
                    <input className="inputo" placeholder="Password" type="password" name="password" minLength={ 14 } required 
                        onInput={ (e) => this.setState( { password: e.currentTarget.value } ) } />
                </div>
                <div className="inputoBorder">
                    <input className="inputo" placeholder="Repeat password" type="password" minLength={ 14 } required
                            onInput={ e => { 
                                if(e.currentTarget.value !== this.state.password)
                                    e.currentTarget.setCustomValidity("Passwords doesn't match");
                                else
                                    e.currentTarget.setCustomValidity("");
                            }}/>
                </div>
                <input className="result" type="submit" value="Sign up" />
            </form>
        );
    }
}
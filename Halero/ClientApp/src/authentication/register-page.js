import React from 'react';
import { toast } from 'react-toastify';

export class Registro extends React.Component{
    constructor(props){
        super(props);

        this.state = {
            userName: null,
            profileName: null,
            email: null,
            password: null
        }
    }

    handleInput(event){
        let target = event.target;
        let name = target.name;
        let value = target.value;

        this.setState({ [name]: value });
    }

    handleSubmit(e){
        e.preventDefault();

        const username      = this.state.username;
        const profileName   = this.state.profileName;
        const email         = this.state.email;
        const password      = this.state.password;

        fetch("/api/authentication/Register", {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                userName: username, 
                userProfileName: profileName,
                email: email,
                password: password
            }) 
          }).then( data => data.json() )
            .then( data => {
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
                    <input className="inputo" placeholder="Username" type="text" name="username" minLength="3" maxLength="30" required 
                        onInput={ (e) => this.handleInput(e) } />
                </div>
                <div className="inputoBorder">
                    <input className="inputo" placeholder="Profile name" type="text" name="profileName" maxLength="30" required 
                        onInput={ (e) => this.handleInput(e) } />
                </div>
                <div className="inputoBorder">
                    <input className="inputo" placeholder="Email" type="email" name="email" required 
                        onInput={ (e) => this.handleInput(e) } />
                </div>
                <div className="inputoBorder">
                    <input className="inputo" placeholder="Password" type="password" name="password" minLength="14" required 
                        onInput={ (e) => this.handleInput(e) } />
                </div>
                <div className="inputoBorder">
                    <input className="inputo" placeholder="Repeat password" type="password" minLength="14" required
                            onInput={ e => { 
                                if(e.target.value != this.state.password)
                                    e.target.setCustomValidity("Passwords doesn't match");
                                else
                                    e.target.setCustomValidity("");
                            }}/>
                </div>
                <input className="result" type="submit" value="Sign up" />
            </form>
        );
    }
}
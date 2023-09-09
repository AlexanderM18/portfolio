import React, { useState, useEffect } from "react";
import { updateFriend, addFriend } from "../../services/friendsService";
import { useParams, useLocation } from "react-router-dom";
import { Formik, Form, useField } from 'formik';
import { friendsValidationSchema } from "../events/validate";
import { uploadFile } from "../../services/eventService";




function AddFriends() {
    const { state } = useLocation()
    const [userFormData, setUserFormData] = useState({ title: " ", bio: " ", summary: " ", headline: " ", slug: " ", ImageUrl: " ", statusId: " ", skills: undefined });
    const { id } = useParams()
    const [friendId, setFriendId] = useState(id)
    const [friendAdded, setFriendAdded] = useState(false);

    useEffect(() => {
        setFriendId(id)
        if (id) {
            setFriendAdded(true);
        }
        if (state?.type === "FRIEND_DATA" && state.payload) {
            const payload = { ...state.payload, ImageUrl: state.payload.primaryImage.url, skills: state.payload.skills?.map(array => array.name).join(", ") };
            setUserFormData((prevState) => { return { ...prevState, ...payload }; });
        };
    }, []);

    const fileSelected = (e) => {
        e.preventDefault()
        const file = e.target.files[0]
        const formData = new FormData();

        formData.append("file", file);

        uploadFile(formData).then(onFileUploadSuccess).catch(onFileUploadError)
        console.log("file: ", file)
    }
    const onFileUploadSuccess = response => {
        console.log("onFileUploadSuccess response: ", response.data.items[0])
        let url = response.data.items[0]
        setUserFormData((prevState) => ({ ...prevState, ImageUrl: url }))
    }
    const onFileUploadError = error => {
        console.log("onFileUploadError: ", error)
    }

    const MyTextInput = ({ label, ...props }) => {
        const [field, meta] = useField(props);
        return (
            <>
                <label htmlFor={props.id || props.name}>{label}</label>
                <input className="text-input" {...field} {...props} />
                {meta.touched && meta.error ? (
                    <div className="error">{meta.error}</div>
                ) : <div className="error-placeholder" />}
            </>
        );
    };
    const MySelect = ({ label, ...props }) => {
        const [field, meta] = useField(props);
        return (
            <div>
                <label htmlFor={props.id || props.name}>{label}</label>
                <select {...field} {...props} />
                {meta.touched && meta.error ? (
                    <div className="error">{meta.error}</div>
                ) : <div className="error-placeholder" />}
            </div>
        );
    };

const formSubmit = (values) => {
                const skillsArray = values.skills?.split(',').map(skill => ({ name: skill.trim() }))
                const finalValues = { ...values, primaryImage: values.ImageUrl, skills: skillsArray && skillsArray[0].name !== "" ? skillsArray : null };
                if (friendAdded) {
                    updateFriend(finalValues, finalValues.id)
                        .then(onUpdateFriendSuccess)
                        .catch(onUpdateFriendError);
                } else {
                    addFriend(finalValues)
                        .then(response => onAddFriendSuccess(response, finalValues))
                        .catch(onAddFriendError);
                }

                function onUpdateFriendSuccess(response) {
                    console.log("onUpdateFriendSuccess: ", response)
                    setUserFormData(prevState => {
                        return {
                            ...prevState,
                        }
                    });
                }

                function onUpdateFriendError(error) {
                    console.warn("onUpdateFriendError: ", error)
                }
                function onAddFriendSuccess(response, values) {
                    console.log("respone: ", response.data.item)
                    const id = response.data.item;
                    setUserFormData(prevState => {
                        return {
                            ...prevState,
                            ...values,
                            id: id
                        }
                    });
                    setFriendAdded(true);
                    toastr.success('Friend added successfully!');
                }

                function onAddFriendError(error) {
                    console.log("error: ", error)
                    toastr.error('Friend add Failed!')
                }
               
            }


    return (<>

        <Formik
            initialValues={userFormData}
            enableReinitialize
            validationSchema={friendsValidationSchema}
            onSubmit={formSubmit}>
            <Form className="f-form-container">
                <div className="cen form-head">{friendAdded || friendId ? (<h1>Update Friend</h1>) : (<h1>Add Friend</h1>)}</div>
                <MyTextInput
                    label="Name"
                    name="title"
                    type="text"
                />
                <MyTextInput
                    label="Bio"
                    name="bio"
                    type="text"
                />
                <MyTextInput
                    label="Summary"
                    name="summary"
                    type="summary"
                />
                <MyTextInput
                    label="Headline"
                    name="headline"
                    type="text"
                />
                <MyTextInput
                    label="Slug"
                    name="slug"
                    type="slug"
                />
                <MyTextInput
                    label="Profile Picture URL"
                    name="ImageUrl"
                    type="text"
                />
                <p>or File</p>
                <input type="file" multiple id="img" name="img" accept="image/png, image/jpeg, multipart/form-data" onChange={fileSelected} />
                <div className="f-place-holder" />
                <MyTextInput
                    label="Skills"
                    name="skills"
                    type="text"
                />
                <MySelect label="Status:&nbsp; " name="statusId" >
                    <option value="">Select a Status</option>
                    <option value="1">Active</option>
                    <option value="2">NotSet</option>
                    <option value="3">Flagged</option>
                    <option value="4">Deleted</option>
                </MySelect>
                <button type="submit" className="f-sub-btn">Submit</button>
            </Form>
        </Formik>
    </>

    )

};

export default AddFriends; 

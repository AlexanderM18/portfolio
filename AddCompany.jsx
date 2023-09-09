import React, { useState, useEffect } from "react";
import { updateCompany, addCompany } from "../../services/companyService";
import { useParams, useLocation } from "react-router-dom";
import { Formik, Form, useField } from "formik";
import { companiesValidationSchema } from "../events/validate";

function AddCompany() {
  const { state } = useLocation();
  const [userFormData, setUserFormData] = useState({
    name: "",
    profile: "",
    summary: "",
    headline: "",
    email: " ",
    phoneNumber: "",
    slug: "",
    images: "",
    urls: [],
    tags: [""],
    id: "",
    statusId: "",
    friendIds: ["0"],
  });
  const { id } = useParams();
  const [companyId, setCompanyId] = useState(id);
  const [companyAdded, setCompanyAdded] = useState(false);
  console.log("form state: ", state);
  useEffect(() => {
    setCompanyId(id);
    if (id) {
      setCompanyAdded(true);
    }
    if (state?.type === "COMPANY_DATA" && state.payload) {
      const payload = {
        ...state.payload,
        email: state.payload.contacts?.map((ary) => ary.email).join(", "),
        phoneNumber: state.payload.contacts?.map((ary) => ary.phoneNumber).join(", "),
        images: state.payload.images?.map((img) => img.url).join(", "),
        tags: state.payload.tags?.map((array) => array.tag).join(", "),
        urls: state.payload.urls?.map((array) => array.url).join(", "),
      };
      setUserFormData((prevState) => {
        return { ...prevState, ...payload };
      });
    }
  }, []);

  console.log("company userformdata: ", userFormData);
  const MyTextInput = ({ label, ...props }) => {
    const [field, meta] = useField(props);
    return (
      <>
        <label htmlFor={props.id || props.name}>{label}</label>
        <input className="text-input" {...field} {...props} />
        {meta.touched && meta.error ? (
          <div className="error">{meta.error}</div>
        ) : (
          <div className="error-placeholder" />
        )}
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
        ) : (
          <div className="error-placeholder" />
        )}
      </div>
    );
  };
  return (
    <>
      <Formik
        initialValues={userFormData}
        enableReinitialize
        validationSchema={companiesValidationSchema}
        onSubmit={(values, { setSubmitting }) => {
          // console.log("submit clicked")
          console.log("values: ", values);
          const finalValues = {
            ...values,
            urls: [values.urls],
            tags: [values.tags],
            images: [{ imageTypeId: 4, url: values.images }],
            contactInfo: [{ Email: values.email, phoneNumber: values.phoneNumber }],
          };
          console.log("finalValues: ", finalValues);
          if (companyAdded) {
            updateCompany(finalValues, finalValues.id)
              .then(onUpdateCompanySuccess)
              .catch(onUpdateCompanyError);
          } else {
            addCompany(finalValues)
              .then((response) => onAddCompanySuccess(response, finalValues))
              .catch(onAddCompanyError);
          }

          function onUpdateCompanySuccess(response) {
            console.log("onUpdateCompanySuccess: ", response);
          }

          function onUpdateCompanyError(error) {
            console.warn("onUpdateCompanyError: ", error);
          }
          function onAddCompanySuccess(response, values) {
            console.log("respone: ", response.data.item);
            const id = response.data.item;
            setUserFormData((prevState) => {
              return {
                ...prevState,
                ...values,
                id: id,
              };
            });
            setCompanyAdded(true);
            setCompanyId(id);
          }

          function onAddCompanyError(error) {
            console.log("error: ", error);
          }
          setTimeout(() => {
            setSubmitting(false);
          }, 400);
        }}>
        <Form className="c-form-container">
          <div className="cen form-head">
            {companyId ? <h1>Update Company</h1> : <h1>Add Company</h1>}
          </div>
          <MyTextInput label="Company Name" name="name" type="text" />
          <MyTextInput label="Profile" name="profile" type="text" />
          <MyTextInput label="Summary" name="summary" type="text" />
          <MyTextInput label="Headline" name="headline" type="text" />
          <MyTextInput label="Company Email Address" name="email" type="email" />
          <MyTextInput label="Company Phone Number" name="phoneNumber" type="text" />
          <MyTextInput label="Slug" name="slug" type="text" />
          <MyTextInput label="Company Logo" name="images" type="text" />
          <MyTextInput label="Company Social Media" name="urls" type="text" />
          <MyTextInput label="Tags" name="tags" type="text" />
          <MySelect label="Status:&nbsp; " name="statusId">
            <option value="">Select a Status</option>
            <option value="Active">Active</option>
            <option value="NotSet">NotSet</option>
            <option value="Flagged">Flagged</option>
            <option value="Deleted">Deleted</option>
          </MySelect>
          <button type="submit">Submit</button>
        </Form>
      </Formik>
    </>
  );
}

export default AddCompany;

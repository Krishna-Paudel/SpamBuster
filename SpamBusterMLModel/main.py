import functions_framework

import pickle
import numpy as np
import pandas as pd
import re
import sklearn

keywords = ['Full refund', 'Cashcashcash', 'Compare rates', 'Billion dollars', 'Call free', 'Only $', 'Xanax', 'Lower monthly payment', 'Best price', 'Cash bonus', 'Hidden assets', 'Order now', 'No obligation', 'They keep your money ? no refund!', 'Explode your business', 'Increase sales', 'This isn?t junk', 'Order today', 'Reserves the right', 'Unsecured credit', 'More Internet Traffic', 'Buying judgments', 'Free leads', 'This isn?t spam', 'Lower interest rate', 'Time limited', 'Get it today', 'Undisclosed recipient', 'No-obligation', 'Lowest price', 'Once in lifetime', 'Cures baldness', 'At no cost', 'Join millions', 'Free grant money', 'Laser printer', 'Real thing', 'Mail in order form', 'Info you requested', 'Get started now', 'Marketing', 'Free DVD', 'Steamy', 'Access now', 'Home employment', 'No medical exams', 'Copy DVDs', 'Visit our website', 'Brand new pager', 'For instant access', 'Your income', 'Free money', 'While supplies last', 'Online business opportunity', 'Be your own boss', 'Not intended', 'Luxury car', 'Increase Income', 'Message contains', 'As seen on', 'Consolidate your debt', 'Apply now', 'Cents on the dollar', 'Risk-free', 'Homebased business', 'No medical exams ', 'Confidentially on all orders', 'Addresses on CD', 'No questions asked', 'Special promotion', 'Free access', 'Satisfaction guaranteed', 'Growth hormone', 'Big bucks', 'Fast cash', 'Month trial offer', 'XXX', 'Make $', 'Save up to', 'One hundred percent guaranteed', 'No investment', 'Work from home', 'Unsubscribe', 'See for yourself', 'All-natural', 'Email harvest', 'Save big', 'The following form', 'Please read', 'Marketing solutions', 'Don?t hesitate', 'Cable converter', 'Dear friend', '50% off', 'Cheap meds', 'Online biz opportunity', 'Why pay more?', 'Calling creditors', 'Eliminate bad credit', 'No refunds', 'Click Here', "Don't hesitate", 'Removal instructions', 'We honor all', 'Internet market', 'Orders shipped by shopper', 'Score with babes', 'Celebrity', 'Free info', "This isn't spam", 'Compete for your business', 'Multi level marketing', '100% satisfaction', 'Very Cheap', 'Subscribe', 'Save $', 'Print out and fax', 'Free cell phone', 'Increase your sales', 'Stock pick', 'Limited time', 'New customers only', 'Financially independent', 'Auto email removal', 'Avoice bankruptcy', "Don't delete", 'Offer expires', 'No inventory', 'Drastically reduced', 'Instant weight loss', 'This is an ad', 'All natural', 'Click to remove', 'No catch', 'Act now!', 'Once in a lifetime', 'Billing address', 'Free investment', 'Potential earnings', 'Join millions of Americans', 'Accept credit cards', 'Free Instant', 'Meet women', 'Dig up dirt on friends', 'Print form signature', 'Unlimited', 'No claim forms', 'Avoid bankruptcy', 'Fast money', 'Get it now', 'You are a winner!', 'Shopping spree', 'Weight loss', 'Free installation', 'Investment decision', 'Free hosting', 'One time', 'For you', 'Click here', 'Stock disclaimer statement', 'Act now', 'Check or money order', '100% satisfied', 'Get started now. ', 'Get paid', 'Cutie', 'Reverses aging', 'What are you waiting for?', 'We hate spam', 'No hidden costs', 'Requires initial investment', 'Weekend getaway', '0% risk', 'Meet girls', 'Extra income', 'Subject to credit', 'Increase traffic', 'Accordingly', 'http://', 'Do it today', 'Risk free', 'Don?t delete', 'Cannot be combined with any other offer', 'Incredible deal', 'Fast Viagra delivery', 'It?s effective', 'Free quote', 'Multi-level marketing', 'If only it were that easy', 'Million dollars', 'Lose weight spam', 'Digital marketing', 'Unsolicited', 'hidden charges', 'No credit check', 'No purchase necessary', 'For just $XXX', 'Guarantee!', 'Join thousands', 'Lowest insurance rates', 'For free', 'Email marketing', 'Fantastic deal', 'Buy now', 'Give it away', 'Search engines', 'This won?t last', 'Sexy babes', 'Expect to earn', 'Action Required', 'University diplomas', 'No age restrictions', 'Lose weight', 'Cards accepted', 'All new', 'For Only', 'Free sample', 'Opportunity', 'Order status', "Can't live without", "This isn't junk", 'Do it now', 'You?ve been selected!', 'Online pharmacy', 'Sign up free today', 'Home based', 'No experience', 'Income from home', 'Act Immediately', 'Have you been turned down?', 'Refinance home', 'Direct marketing', 'Home-based', 'Information you requested', 'Near you', 'Lower your mortgage rate', 'Bulk email', 'New domain extensions', 'You have been selected', 'Can?t live without', 'Click below', 'Earn $', 'Dear [email/friend/somebody]', 'F r e e', 'Save big money', 'While you sleep', 'Free website', 'Free membership', 'Double your', '$$$', 'Pennies a day', 'Being a member', 'Who really wins?', 'Card accepted', 'Get rid of debt', 'Will not believe your eyes', 'Free gift', 'Meet singles', 'You are a winner', 'Unsecured debt', 'Financial freedom', 'Human growth hormone', 'Serious cash', 'Make money', 'Money making', 'Deal ending soon', 'No purchase required', 'Priority mail', 'No fees', 'Call now', 'They?re just giving it away', 'Stainless steel', 'No middleman', 'Pure profit', 'Earn per week', 'Credit bureaus', 'Money back', 'One time mailing', 'Free priority mail', 'US dollars', 'Removes wrinkles', 'No cost', 'Stock alert', 'Produced and sent out', '100% more', 'Free preview', 'Now only', 'Free trial', 'No selling', 'Sent in compliance', 'Vacation offers', 'No hidden Costs', 'Earn cash', 'Online degree', 'The best rates', 'In accordance with laws', 'You?re a Winner!', 'Apply online', 'Stops snoring', 'Acceptance', 'Earn extra cash', 'All-new', 'Terms and conditions', 'Social security number', 'Gift certificate', 'Long distance phone offer', 'Stuff on sale', 'Work at home', 'Hottie', 'Copy accurately', 'Important information regarding', 'Get out of debt', 'No gimmick', 'Free offer', 'No strings attached', 'Supplies are limited', 'Name brand', 'Outstanding values', 'Mortgage rates', 'Cancel at any time', 'Take action now', 'Free consultation', 'Easy terms', 'Internet marketing', 'Credit card offers', '100% free', 'Consolidate debt and credit', 'Hurry up', 'Additional income', 'No disappointment', 'Take action', 'Mass email', 'While stocks last', 'Hot babes', 'Double your income', 'Web traffic', 'Great offer', 'Kinky', 'One hundred percent free', 'Exclusive deal', 'Collect child support', 'Eliminate debt', 'Online marketing', 'Sex', 'Search engine listings', 'Life insurance', 'Stop snoring', 'Giving away', 'Promise you', 'Buy direct']



@functions_framework.http
def spambusterapi(request):
    """HTTP Cloud Function.
    Args:
        request (flask.Request): The request object.
        <https://flask.palletsprojects.com/en/1.1.x/api/#incoming-request-data>
    Returns:
        The response text, or any set of values that can be turned into a
        Response object using `make_response`
        <https://flask.palletsprojects.com/en/1.1.x/api/#flask.make_response>.
    """
    request_json = request.get_json(silent=True)
    request_args = request.args

    if request_json and 'SPFFlag' in request_json:
        SPFFlag = request_json['SPFFlag']
    elif request_args and 'SPFFlag' in request_args:
        SPFFlag = request_args['SPFFlag']
    else:
        SPFFlag = ''

    if request_json and 'Subject' in request_json:
        Subject = request_json['Subject']
    elif request_args and 'Subject' in request_args:
        Subject = request_args['Subject']
    else:
        Subject = ''
        
    if request_json and 'Body' in request_json:
        Body = request_json['Body']
    elif request_args and 'Body' in request_args:
        Body = request_args['Body']
    else:
        Body = ''

    if request_json and 'DKIMFlag' in request_json:
        DKIMFlag = request_json['DKIMFlag']
    elif request_args and 'DKIMFlag' in request_args:
        DKIMFlag = request_args['DKIMFlag']
    else:
        DKIMFlag = ''

    if request_json and 'DMARCFlag' in request_json:
        DMARCFlag = request_json['DMARCFlag']
    elif request_args and 'DMARCFlag' in request_args:
        DMARCFlag = request_args['DMARCFlag']
    else:
        DMARCFlag = ''        
    
    if SPFFlag == '' or Body == '' or Subject == '' or DKIMFlag == '' or DMARCFlag == '':
        return 'ValueMissing: {}'.format(404)

    try:   
      
      
      x_evaluation = prepareInputData(Subject, SPFFlag, DKIMFlag, DMARCFlag, Body)

      x_evaluate = pd.DataFrame(x_evaluation)
      # preprocess the input data and make predictions using the function
      result = preprocess_and_predict(x_evaluate,'labelencoder_model.pkl','onehotencoder_model.pkl','countvectorizer_model.pkl','ann_model.pkl')

               

      # Output either 'Negative' or 'Positive' along with the score
      if result > 0.5:
          return 'Output: Ham'
      else:
          return 'Output: Spam'
    except Exception as e:
      print(e)
      return 'Exception: {}'.format(e)


def clean_subject(subject):
    # Remove all non-alphanumeric characters except for $ % : ; , and space
    subject = re.sub(r'[^a-zA-Z0-9$%:;, ]', '', subject)
    # Remove any leading or trailing whitespace
    subject = subject.strip()
    subject = ''.join(subject)
    return subject


def prepareInputData(subject, SPFFlag, DKIMFlag, DMARCFlag, body):
    try:                       

        # Clean the subject
        subject = clean_subject(subject)

        body = body.replace('\n', ' ')

        # Check if email body has spam keywords
        body_has_keywords = False
        for keyword in keywords:
            if keyword.lower() in body.lower():
                body_has_keywords = True
                break
            
        # Categorize email as spam or non-spam
        if body_has_keywords:
            word_pattern = "Has spam patterns"
        else:
            word_pattern = "Non-spam patterns"

        # Write the extracted data into a dictionary
        my_dict = [{'Subject': subject,
                'SPF': SPFFlag.lower(),
                'DKIM': DKIMFlag.lower(),
                'DMARC': DMARCFlag.lower(),
                'Word pattern': word_pattern}]

        return my_dict
    except:
        return None
    

def preprocess_and_predict(x_evaluate, le_path, onehot_enc_path, cv_path, ann_path):
    # load label encoder model
    with open(le_path, 'rb') as f:
        le = pickle.load(f)      

    # encode 'Word pattern' column using label encoder
    x_evaluate['Word pattern'] = le.transform(x_evaluate['Word pattern'])

        # load one-hot encoder model
    with open(onehot_enc_path, 'rb') as f:
        onehot_enc = pickle.load(f)  

    # encode categorical columns using one-hot encoder
    cat_cols = ['SPF', 'DKIM', 'DMARC']
    x_evaluate_cat = onehot_enc.transform(x_evaluate[cat_cols])

    # remove categorical columns from the input data and concatenate one-hot encoded columns
    x_evaluate = x_evaluate.drop(cat_cols, axis=1)
    x_evaluate = np.hstack((x_evaluate.values, x_evaluate_cat.toarray()))

    # load count vectorizer model
    with open(cv_path, 'rb') as f:
        cv = pickle.load(f) 

    # vectorize 'Subject' column using count vectorizer
    x_evaluate_subject = cv.transform(x_evaluate[:,0])  # index 0 corresponds to 'Subject' column
    x_evaluate_subject = x_evaluate_subject.toarray()
    
    # concatenate the vectorized 'Subject' column with the rest of the preprocessed data
    x_evaluate = np.hstack((x_evaluate_subject, x_evaluate[:, 1:]))

    # convert the preprocessed data to a tensor
    #x_evaluate = tf.convert_to_tensor(x_evaluate, dtype=tf.float32)

    # load ANN model
    with open(ann_path, 'rb') as f:
        ann_model = pickle.load(f)  
    # make predictions on the preprocessed data using the ANN model

    y_pred = ann_model.predict(x_evaluate)
    # compare the predicted probabilities to a threshold of 0.5 and return the predicted label
    return y_pred

